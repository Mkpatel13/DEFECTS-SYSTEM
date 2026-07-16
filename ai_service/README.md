# YOLOv8 Defect Detection Microservice

This is a FastAPI-based microservice that performs defect detection using the YOLOv8 model. 

## Features
- **Health Check Endpoint**: `GET /health` to monitor service status.
- **Inference Endpoint**: `POST /predict` accepts an image via multipart form upload, runs YOLOv8 inference, saves the annotated image to the `detected_images/` folder, and returns the highest confidence detection.
- **Fallback Capability**: If custom weights are not found at the path defined by the `MODEL_PATH` environment variable (default: `models/best.pt`), the service automatically falls back to `yolov8n.pt` (the pre-trained COCO-based nano model) to allow end-to-end pipeline testing.

---

## Directory Structure
```text
ai_service/
├── app/
│   ├── __init__.py
│   ├── main.py         # FastAPI application entrypoint & routes
│   ├── model.py        # YOLOv8 loading & inference logic
│   └── schemas.py      # Pydantic response models
├── data/               # Clean structured dataset (created by prepare_dataset.py)
│   ├── raw/            # Raw downloaded dataset
│   ├── images/         # Clean structured image folders (train/val)
│   ├── labels/         # Clean structured label files (train/val)
│   └── data.yaml       # Final dataset configurations
├── detected_images/    # Directory where annotated images will be saved
├── prepare_dataset.py  # Dataset downloader and class remapper
├── requirements.txt    # Project dependencies
└── README.md           # Documentation
```

---

## Setup and Installation

### 1. Prerequisites
Make sure you have Python 3.8+ installed on your system.

### 2. Create and Activate a Virtual Environment
Run the following commands in your terminal:

**Windows (PowerShell):**
```powershell
python -m venv venv
.\venv\Scripts\Activate.ps1
```

**Linux / macOS:**
```bash
python3 -m venv venv
source venv/bin/activate
```

### 3. Install Dependencies
```bash
pip install --upgrade pip
pip install -r requirements.txt
```

### 4. Prepare Dataset (Optional)
To train the model on PCB defects, we provide a dataset downloader and remapping utility:

1. Ensure the `roboflow` package is installed:
   ```bash
   pip install roboflow
   ```
2. Run the preparation script by providing your Roboflow API key:
   ```bash
   python prepare_dataset.py --api-key YOUR_ROBOFLOW_API_KEY
   ```
   This will:
   - Download the raw PCB defect dataset into `data/raw/`.
   - Prepare and map indices to our 6 target classes:
     - `missing_hole` -> `missing_hole` (0)
     - `mouse_bite` -> `mouse_bite` (1)
     - `open_circuit` -> `open_circuit` (2)
     - `short` / `short_circuit` -> `short_circuit` (3)
     - `spur` -> `spur` (4)
     - `spurious_copper` -> `spurious_copper` (5)
   - Restructure images and labels splits into `data/images/{train,val}` and `data/labels/{train,val}` (merging test into validation).
   - Write `data/data.yaml` pointing to the new directories.
   - Print a distribution report of images/instances per class.

---

## Running the Microservice

### Method 1: Running with default weights (fallback to yolov8n.pt)
If you don't have custom weights ready yet, just run:
```bash
uvicorn app.main:app --reload --port 8000
```
Upon startup, the app will log a warning stating that `models/best.pt` was not found, download `yolov8n.pt` from Ultralytics, and run inference using it.

### Method 2: Running with custom weights
1. Create a `models/` directory (or any custom path) and copy your custom YOLOv8 `.pt` file there.
2. Set the `MODEL_PATH` environment variable and start the application:

**Windows (PowerShell):**
```powershell
$env:MODEL_PATH="models/best.pt"
uvicorn app.main:app --reload --port 8000
```

**Linux / macOS:**
```bash
MODEL_PATH="models/best.pt" uvicorn app.main:app --reload --port 8000
```

---

## API Documentation and Testing

Once the server is running, you can access the interactive API docs at:
- Swagger UI: [http://localhost:8000/docs](http://localhost:8000/docs)
- ReDoc: [http://localhost:8000/redoc](http://localhost:8000/redoc)

### Testing with `curl`

To test the endpoint using a sample image, execute the following command:

**Windows (PowerShell or Command Prompt):**
```powershell
curl.exe -X POST "http://127.0.0.1:8000/predict" -H "accept: application/json" -H "Content-Type: multipart/form-data" -F "file=@path/to/your/image.jpg"
```

**Linux / macOS:**
```bash
curl -X POST "http://127.0.0.1:8000/predict" \
     -H "accept: application/json" \
     -H "Content-Type: multipart/form-data" \
     -F "file=@path/to/your/image.jpg"
```

### Example JSON Response
```json
{
  "defectType": "class_name",
  "confidence": 0.894,
  "isDefective": true,
  "detectedImagePath": "detected_images/image_abcdef12_annotated.jpg"
}
```
If no objects/defects are detected:
```json
{
  "defectType": "none",
  "confidence": 0.0,
  "isDefective": false,
  "detectedImagePath": null
}
```
