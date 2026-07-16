# PCB Defect Inspection System

This project is an automated PCB defect detection and quality control system built using a YOLOv8 object detection microservice, a Spring Boot REST API layer, a MySQL database, and a React dashboard interface.

---

## 🧠 AI Model Specification (EdgePCB YOLOv8)

The system utilizes the fine-tuned **EdgePCB YOLOv8** model, pre-trained on a dataset of **~28,000 PCB images** and optimized for real-time edge device inference. 

* **Architecture**: YOLOv8 (Ultralytics)
* **Weights File**: Saved at `ai_service/models/best.pt` (automatically resolved on startup, downloaded from [Hugging Face](https://huggingface.co/Tanishjain9/pcb-defect-detection-yolov8/resolve/9420f0abe215cfb2f1dea26e3816dbfc2e94ffc0/best.pt))
* **Detection Categories (6 classes)**:
  1. **Missing Hole** (`missing_hole`)
  2. **Mouse Bite** (`mouse_bite`)
  3. **Open Circuit** (`open_circuit`)
  4. **Short Circuit** (`short_circuit` / normalized from raw `short`)
  5. **Spur** (`spur`)
  6. **Spurious Copper** (`spurious_copper`)

---

## System Architecture

The project consists of three main modules communicating in real-time:
1. **React Dashboard (Frontend)**: Runs on port `3000`. It allows visual board uploads, triggers inspections, and displays defect statistics and real-time history.
2. **Spring Boot Microservice (Backend)**: Runs on port `8081`. It manages SQL logs, calculates statistics, and orchestrates calls to the Python YOLOv8 service.
3. **YOLOv8 defect detector (AI Service)**: Runs on port `8000`. It loads fine-tuned model weights (using data augmentation) and runs high-speed object detection.

```
       +------------------------------------+
       |          React Dashboard           | (Port 3000)
       +-----------------+------------------+
                         |
                         | Upload PCB image & select SKU
                         v
       +-----------------+------------------+
       |        Spring Boot Backend         | (Port 8081)
       +--------+------------------+--------+
                |                  |
   Save log     |                  | Forward image
   to database  v                  v
       +--------+---+      +-------+--------+
       |  MySQL DB  |      |   AI Service   | (Port 8000, FastAPI + YOLOv8)
       +------------+      +----------------+
```

---

## Directory Structure

```text
defect-inspection-system/
├── ai_service/             # YOLOv8 FastAPI prediction engine & training pipeline
│   ├── app/                # Server source code
│   ├── data/               # PCB defects training dataset
│   ├── models/             # fine-tuned model weights (best.pt)
│   ├── prepare_dataset.py  # Dataset downloader and class remapper
│   └── train.py            # Model training & synthetic data augmentation script
├── backend/                # Spring Boot REST API
│   ├── src/                # Spring Boot java source code & database schema
│   └── pom.xml             # Maven dependencies
├── frontend/               # React operator dashboard
│   ├── src/                # React dashboard source code
│   └── package.json        # Frontend dependencies
└── docs/                   # Project documentation & Synopsis
    └── architecture_diagram.png
```

---

## How to Run the System

### Prerequisites
Make sure you have the following installed on your machine:
- Python 3.8+
- Java 17+ & Maven
- Node.js & npm
- MySQL Server (running on port 3306)

---

### Step 1: Start the YOLOv8 AI Microservice
Navigate to the `ai_service` folder, activate the virtual environment, install requirements, and run the FastAPI server:

```bash
cd ai_service
pip install -r requirements.txt
# Start the server (loads models/best.pt)
uvicorn app.main:app --reload --port 8000
```
*The AI service will start listening on port `8000`.*

---

### Step 2: Start the Spring Boot Backend
1. Ensure your local MySQL server is running and update `backend/src/main/resources/application.properties` with your database username and password (default: `root`/`root`).
2. Run the application using Maven:

```bash
cd backend
mvn clean spring-boot:run
```
*The backend will automatically create the `defect_inspection_db` schema, seed the default products, and start listening on port `8081`.*

---

### Step 3: Start the React Frontend Dashboard
Navigate to the `frontend` folder, install npm dependencies, and launch the server:

```bash
cd frontend
npm install
npm start
```
*The dashboard will automatically open in your browser at `http://localhost:3000`.*

---

## defect-inspection-system APIs
* **FastAPI documentation**: `http://localhost:8000/docs` (Swagger UI)
* **Spring Boot endpoint stats**: `http://localhost:8081/api/inspections/stats` (JSON)
