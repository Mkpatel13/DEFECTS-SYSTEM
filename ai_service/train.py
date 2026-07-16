import os
import sys
import shutil
import argparse
from pathlib import Path
import numpy as np
from PIL import Image, ImageEnhance
import yaml
from ultralytics import YOLO

def parse_args():
    parser = argparse.ArgumentParser(description="Train YOLOv8 on custom PCB defect dataset.")
    parser.add_argument("--epochs", type=int, default=50, help="Number of training epochs.")
    parser.add_argument("--batch", type=int, default=16, help="Batch size for training.")
    parser.add_argument("--imgsz", type=int, default=640, help="Input image size.")
    parser.add_argument("--device", type=str, default="", help="Device to train on (e.g. 0, cpu).")
    parser.add_argument("--no-aug", action="store_true", help="Disable synthetic offline augmentation.")
    return parser.parse_args()

def clean_previous_augmentations(images_dir, labels_dir):
    """
    Cleans up any previously generated augmented files to ensure idempotency.
    """
    print("Cleaning previous synthetic augmentations...")
    aug_patterns = ["*_aug_bright*", "*_aug_noise*", "*_aug_hflip*", "*_aug_vflip*"]
    
    deleted_images = 0
    deleted_labels = 0
    
    for pattern in aug_patterns:
        for file in images_dir.glob(pattern):
            file.unlink()
            deleted_images += 1
        for file in labels_dir.glob(pattern):
            file.unlink()
            deleted_labels += 1
            
    print(f"  Removed {deleted_images} augmented images and {deleted_labels} augmented label files.")

def augment_dataset(images_dir, labels_dir):
    """
    Applies synthetic offline transformations (brightness, noise, flips) to the training set.
    """
    print("\nApplying synthetic data augmentation...")
    
    # Clean previous runs first
    clean_previous_augmentations(images_dir, labels_dir)
    
    # Locate original images (images not containing '_aug_')
    original_images = [f for f in images_dir.glob("*") if f.is_file() and "_aug_" not in f.name]
    print(f"Found {len(original_images)} original training images.")
    
    augmented_count = 0
    
    for img_path in original_images:
        img_name = img_path.name
        img_stem = img_path.stem
        img_ext = img_path.suffix
        
        # Load image
        try:
            img = Image.open(img_path)
            img.load()  # Force load into memory
        except Exception as e:
            print(f"  Warning: Failed to load {img_path}: {e}")
            continue
            
        # Load boxes if label file exists
        lbl_path = labels_dir / f"{img_stem}.txt"
        boxes = []
        if lbl_path.exists():
            with open(lbl_path, "r") as f:
                for line in f:
                    parts = line.strip().split()
                    if len(parts) == 5:
                        try:
                            class_id = int(parts[0])
                            x = float(parts[1])
                            y = float(parts[2])
                            w = float(parts[3])
                            h = float(parts[4])
                            boxes.append((class_id, x, y, w, h))
                        except ValueError:
                            continue
                            
        # 1. Transform: Brightness (Scale pixel intensity by 1.3)
        try:
            enhancer = ImageEnhance.Brightness(img)
            bright_img = enhancer.enhance(1.3)
            bright_img.save(images_dir / f"{img_stem}_aug_bright{img_ext}")
            # Labels remain the same
            shutil.copy2(lbl_path, labels_dir / f"{img_stem}_aug_bright.txt") if lbl_path.exists() else None
            augmented_count += 1
        except Exception as e:
            print(f"  Error generating brightness transform for {img_name}: {e}")
            
        # 2. Transform: Random Gaussian Noise
        try:
            img_arr = np.array(img).astype(np.int16)
            noise = np.random.normal(0, 15, img_arr.shape).astype(np.int16)
            noise_arr = np.clip(img_arr + noise, 0, 255).astype(np.uint8)
            noise_img = Image.fromarray(noise_arr)
            noise_img.save(images_dir / f"{img_stem}_aug_noise{img_ext}")
            # Labels remain the same
            shutil.copy2(lbl_path, labels_dir / f"{img_stem}_aug_noise.txt") if lbl_path.exists() else None
            augmented_count += 1
        except Exception as e:
            print(f"  Error generating noise transform for {img_name}: {e}")

        # 3. Transform: Horizontal Flip
        try:
            hflip_img = img.transpose(Image.FLIP_LEFT_RIGHT)
            hflip_img.save(images_dir / f"{img_stem}_aug_hflip{img_ext}")
            # Flip x_center: x_new = 1.0 - x_old
            hflip_lines = []
            for class_id, x, y, w, h in boxes:
                hflip_lines.append(f"{class_id} {1.0 - x:.6f} {y:.6f} {w:.6f} {h:.6f}")
            with open(labels_dir / f"{img_stem}_aug_hflip.txt", "w") as f_out:
                f_out.write("\n".join(hflip_lines) + "\n")
            augmented_count += 1
        except Exception as e:
            print(f"  Error generating horizontal flip for {img_name}: {e}")

        # 4. Transform: Vertical Flip
        try:
            vflip_img = img.transpose(Image.FLIP_TOP_BOTTOM)
            vflip_img.save(images_dir / f"{img_stem}_aug_vflip{img_ext}")
            # Flip y_center: y_new = 1.0 - y_old
            vflip_lines = []
            for class_id, x, y, w, h in boxes:
                vflip_lines.append(f"{class_id} {x:.6f} {1.0 - y:.6f} {w:.6f} {h:.6f}")
            with open(labels_dir / f"{img_stem}_aug_vflip.txt", "w") as f_out:
                f_out.write("\n".join(vflip_lines) + "\n")
            augmented_count += 1
        except Exception as e:
            print(f"  Error generating vertical flip for {img_name}: {e}")

    print(f"Augmentation complete! Generated {augmented_count} synthetic training samples.")

def main():
    args = parse_args()
    
    script_dir = Path(__file__).resolve().parent
    data_dir = script_dir / "data"
    data_yaml = data_dir / "data.yaml"
    models_dir = script_dir / "models"
    models_dir.mkdir(parents=True, exist_ok=True)
    
    if not data_yaml.exists():
        print(f"Error: Could not find dataset config file at {data_yaml}")
        print("Please run prepare_dataset.py first to construct the dataset.")
        sys.exit(1)
        
    train_images_dir = data_dir / "images" / "train"
    train_labels_dir = data_dir / "labels" / "train"
    
    # Run augmentation if requested
    if not args.no_aug:
        augment_dataset(train_images_dir, train_labels_dir)
        
    # Get total training sample count
    total_images = len(list(train_images_dir.glob("*")))
    print(f"\nFinal training dataset size: {total_images} images (including synthetic samples).")
    
    # Initialize model (Load pretrained YOLOv8 nano model)
    print("\nInitializing YOLOv8 model training...")
    model = YOLO("yolov8n.pt")
    
    # Build training configurations
    train_kwargs = {
        "data": str(data_yaml.resolve().as_posix()),
        "epochs": args.epochs,
        "batch": args.batch,
        "imgsz": args.imgsz,
        "project": "runs",
        "name": "defect_train",
        "exist_ok": True
    }
    
    # Handle device parameter
    if args.device:
        if args.device.lower() == "cpu":
            train_kwargs["device"] = "cpu"
        else:
            train_kwargs["device"] = args.device

    # Run fine-tuning
    print(f"Running YOLOv8 fine-tuning for {args.epochs} epochs...")
    model.train(**train_kwargs)
    
    # Copy best weights to models/best.pt
    # Retrieve save directory dynamically from model trainer to avoid hardcoding issues with global settings
    save_dir = Path(model.trainer.save_dir) if hasattr(model, 'trainer') and model.trainer else Path("runs/defect_train")
    best_weights_path = save_dir / "weights" / "best.pt"
    target_weights_path = models_dir / "best.pt"
    
    if best_weights_path.exists():
        print(f"\nTraining completed! Copying weights from '{best_weights_path}' to '{target_weights_path}'...")
        shutil.copy2(best_weights_path, target_weights_path)
        print("Weights copy completed successfully.")
    else:
        # Fallback: check hardcoded project path
        hardcoded_path = Path("runs") / "defect_train" / "weights" / "best.pt"
        if hardcoded_path.exists():
            print(f"\nTraining completed! Copying weights from fallback '{hardcoded_path}' to '{target_weights_path}'...")
            shutil.copy2(hardcoded_path, target_weights_path)
            print("Weights copy completed successfully.")
        else:
            print(f"\nError: Could not find trained weights file at '{best_weights_path}' or '{hardcoded_path}'")
            sys.exit(1)

if __name__ == "__main__":
    main()
