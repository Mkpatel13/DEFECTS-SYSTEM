# Defect Inspection Dashboard (React)

This is the frontend dashboard that serves as the operator panel for running real-time PCB quality checks and monitoring line defect analytics.

## Features
- **Apple-Inspired Design**: Sleek glassmorphism acrylic cards, SF Pro typography, smooth micro-interactions, and premium Apple color palettes.
- **Light & Dark Mode Switch**: Toggle between Apple Dark (obsidian `#000000`) and Light (`#f5f5f7`) modes with preference saved in `localStorage`.
- **Drag-and-Drop Upload**: File selection for PCB board images with visual dropzone states.
- **YOLOv8 & Backend Integration**: Connects with Spring Boot and FastAPI AI microservice.
- **Dynamic Chart Analytics**: Defect distribution bar chart using Chart.js with theme-adaptive colors and typography.
- **Inspection History Table & Modal**: Live data table with badge statuses, Admin deletion capabilities, and annotated bounding box modal viewer.

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
