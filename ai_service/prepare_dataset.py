import os
import sys
import shutil
import argparse
from pathlib import Path
from collections import defaultdict
import yaml
from roboflow import Roboflow

def parse_args():
    parser = argparse.ArgumentParser(description="Download and prepare PCB defects dataset.")
    parser.add_argument(
        "--api-key",
        type=str,
        default=os.getenv("ROBOFLOW_API_KEY"),
        help="Roboflow API key. Can also be set via the ROBOFLOW_API_KEY environment variable."
    )
    return parser.parse_args()

def main():
    args = parse_args()
    api_key = args.api_key

    # Attempt to load from Roboflow config directory if not provided
    if not api_key:
        try:
            import json
            config_path = Path.home() / ".config" / "roboflow" / "config.json"
            if config_path.exists():
                with open(config_path, "r") as f:
                    api_key = json.load(f).get("API_KEY")
        except Exception:
            pass

    if not api_key:
        print("Error: Roboflow API key is required.")
        print("Please set the ROBOFLOW_API_KEY environment variable, pass it via --api-key,")
        print("or authenticate locally using 'roboflow login'.")
        print("You can obtain your key from: https://app.roboflow.com/")
        sys.exit(1)

    # Directory layout setup
    script_dir = Path(__file__).resolve().parent
    data_dir = script_dir / "data"
    raw_dir = data_dir / "raw"

    data_dir.mkdir(parents=True, exist_ok=True)
    if raw_dir.exists():
        print(f"Raw directory '{raw_dir}' already exists. Skipping Roboflow download and reusing existing raw dataset files.")
    else:
        if not api_key:
            print("Error: Roboflow API key is required to download the dataset.")
            print("Please set the ROBOFLOW_API_KEY environment variable or pass --api-key.")
            sys.exit(1)
        print("Initializing Roboflow client...")
        rf = Roboflow(api_key=api_key)
        
        print("Downloading 'pcb-defects-detection-yolov8' dataset in YOLOv8 format...")
        try:
            # Use relative path where possible to avoid Windows path parser / space issues
            try:
                raw_path_str = os.path.relpath(raw_dir, os.getcwd())
            except ValueError:
                raw_path_str = raw_dir.as_posix()
                
            print(f"Downloading to: {raw_path_str}")
            project = rf.workspace("biancapcbdefects").project("pcb-defects-detection-yolov8")
            dataset = project.version(1).download("yolov8", location=raw_path_str)
        except BaseException as e:
            print(f"Error downloading dataset: {type(e).__name__}: {e}")
            import traceback
            traceback.print_exc()
            sys.exit(1)

    raw_yaml_path = raw_dir / "data.yaml"
    if not raw_yaml_path.exists():
        print(f"Error: Could not find raw data.yaml at {raw_yaml_path}")
        sys.exit(1)

    # Parse raw configuration yaml
    with open(raw_yaml_path, "r") as f:
        raw_yaml = yaml.safe_load(f)

    original_classes = raw_yaml.get("names", [])
    print(f"\nOriginal classes detected in raw dataset: {original_classes}")

    # Define Target Classes in the required order
    TARGET_CLASSES = [
        "missing_hole",
        "mouse_bite",
        "open_circuit",
        "short_circuit",
        "spur",
        "spurious_copper"
    ]

    # Map original names to target class names
    CLASS_MAPPING = {
        "missing_hole": "missing_hole",
        "mouse_bite": "mouse_bite",
        "open_circuit": "open_circuit",
        "short": "short_circuit",
        "short_circuit": "short_circuit",
        "spur": "spur",
        "spurious_copper": "spurious_copper"
    }

    # Normalize name to avoid mismatches due to spacing/dashes/casing
    def normalize(name):
        return name.lower().strip().replace("-", "_").replace(" ", "_")

    # Map original indices to target indices
    original_to_target_idx = {}
    print("\nClass Mapping Analysis:")
    for orig_idx, orig_name in enumerate(original_classes):
        norm_orig = normalize(orig_name)
        mapped_target = None

        # Check explicit mapping first
        for src_name, tgt_name in CLASS_MAPPING.items():
            if normalize(src_name) == norm_orig:
                mapped_target = tgt_name
                break

        # Fallback: check if matches target class directly
        if mapped_target is None:
            for tgt_name in TARGET_CLASSES:
                if normalize(tgt_name) == norm_orig:
                    mapped_target = tgt_name
                    break

        if mapped_target is not None:
            target_idx = TARGET_CLASSES.index(mapped_target)
            original_to_target_idx[orig_idx] = target_idx
            print(f"  [Match] Original '{orig_name}' (idx {orig_idx}) -> Target '{mapped_target}' (idx {target_idx})")
        else:
            print(f"  [Ignore] Original '{orig_name}' (idx {orig_idx}) does not map to any target class.")

    # Target folder locations
    final_images_train = data_dir / "images" / "train"
    final_images_val = data_dir / "images" / "val"
    final_labels_train = data_dir / "labels" / "train"
    final_labels_val = data_dir / "labels" / "val"

    # Recreate structured folders
    for d in [final_images_train, final_images_val, final_labels_train, final_labels_val]:
        if d.exists():
            shutil.rmtree(d)
        d.mkdir(parents=True, exist_ok=True)

    # Process and remap splits (merge test split into val to maximize validation size)
    splits_mapping = {
        "train": (final_images_train, final_labels_train),
        "valid": (final_images_val, final_labels_val),
        "test": (final_images_val, final_labels_val)
    }

    # Stat counters
    images_per_class_total = defaultdict(set)
    images_per_class_train = defaultdict(set)
    images_per_class_val = defaultdict(set)
    instances_per_class = defaultdict(int)
    total_images_processed = 0

    print("\nProcessing and copying dataset files...")
    for split_name, (tgt_img_dir, tgt_lbl_dir) in splits_mapping.items():
        split_img_dir = raw_dir / split_name / "images"
        split_lbl_dir = raw_dir / split_name / "labels"

        if not split_img_dir.exists():
            continue

        print(f"  Reading '{split_name}' split...")
        for img_path in split_img_dir.glob("*"):
            if not img_path.is_file():
                continue

            img_name = img_path.name
            base_name = img_path.stem
            lbl_path = split_lbl_dir / f"{base_name}.txt"

            tgt_img_path = tgt_img_dir / img_name
            tgt_lbl_path = tgt_lbl_dir / f"{base_name}.txt"

            # Copy Image
            shutil.copy2(img_path, tgt_img_path)

            # Read, Remap and Write Labels
            img_classes = set()
            new_lines = []

            if lbl_path.exists():
                with open(lbl_path, "r") as f_lbl:
                    for line in f_lbl:
                        parts = line.strip().split()
                        if not parts:
                            continue
                        try:
                            orig_idx = int(parts[0])
                        except ValueError:
                            continue

                        if orig_idx in original_to_target_idx:
                            tgt_idx = original_to_target_idx[orig_idx]
                            parts[0] = str(tgt_idx)
                            new_lines.append(" ".join(parts))
                            img_classes.add(tgt_idx)
                            instances_per_class[tgt_idx] += 1

            # Write out annotations file
            with open(tgt_lbl_path, "w") as f_lbl_out:
                if new_lines:
                    f_lbl_out.write("\n".join(new_lines) + "\n")
                else:
                    # Write empty file for background images without labels
                    f_lbl_out.write("")

            # Update stats
            total_images_processed += 1
            for tgt_idx in img_classes:
                images_per_class_total[tgt_idx].add(img_name)
                if tgt_img_dir == final_images_train:
                    images_per_class_train[tgt_idx].add(img_name)
                else:
                    images_per_class_val[tgt_idx].add(img_name)

    # Write target data.yaml
    new_yaml = {
        "path": str(data_dir.resolve().as_posix()),
        "train": "images/train",
        "val": "images/val",
        "names": {idx: name for idx, name in enumerate(TARGET_CLASSES)}
    }

    final_yaml_path = data_dir / "data.yaml"
    with open(final_yaml_path, "w") as f_yaml:
        yaml.safe_dump(new_yaml, f_yaml, sort_keys=False)

    # Print summary
    print("\n" + "="*60)
    print("               PCB DEFECTS DATASET PREPARATION SUMMARY")
    print("="*60)
    print(f"Total Images Copying/Processing: {total_images_processed}")
    print(f"Dataset path saved in data.yaml: {new_yaml['path']}")
    print("\nDistribution of Images containing at least one defect per class:")
    for tgt_idx, name in enumerate(TARGET_CLASSES):
        train_cnt = len(images_per_class_train[tgt_idx])
        val_cnt = len(images_per_class_val[tgt_idx])
        tot_cnt = len(images_per_class_total[tgt_idx])
        inst_cnt = instances_per_class[tgt_idx]
        
        print(f"  Class {tgt_idx} [{name}]:")
        print(f"    Train Images:       {train_cnt}")
        print(f"    Validation Images:  {val_cnt}")
        print(f"    Total Images:       {tot_cnt}")
        print(f"    Total Annotations:  {inst_cnt}")
        print("-" * 45)

if __name__ == "__main__":
    main()
