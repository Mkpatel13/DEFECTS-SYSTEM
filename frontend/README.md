# Defect Inspection Dashboard (React)

This is the frontend dashboard that serves as the operator panel for running real-time PCB quality checks and monitoring line defect analytics.

## Features
- Drag-and-drop or file upload selection for PCB board images.
- Integrates with the Spring Boot backend service.
- Visual display cards tracking Inspected Count, Defect Count, and Rate.
- Live data logging table showing inspection history with defect-type badges.
- Image modal viewer rendering annotated bounding boxes served dynamically.
- Interactive defect distribution chart built via Chart.js.

## Setup Instructions

1. **Install Dependencies**:
   Execute the following command in the `frontend/` directory to download required packages:
   ```bash
   npm install
   ```

2. **Start Development Server**:
   Launch the dashboard locally:
   ```bash
   npm start
   ```
   The React dashboard opens automatically in your browser at `http://localhost:3000`.

## Configurations
- Requests are routed to the backend at `http://localhost:8081/api` (as configured in `src/api/inspectionApi.js`).
