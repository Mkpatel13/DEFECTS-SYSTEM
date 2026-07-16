# Defect Inspection Backend (Spring Boot)

This is the backend microservice that bridges the React dashboard and the Python YOLOv8 microservice, managing defect logs, database queries, and dashboard metrics.

## Features
- Exposes REST APIs for React frontend integration.
- Resolves and saves PCB products and inspection logs to a MySQL database.
- Integrates with the Python YOLOv8 service via Spring `WebClient` for classification checks.
- Aggregates defect stats dynamically for the dashboard interface.

## Tech Stack
- Java 17
- Spring Boot 3.2.5
- Spring Data JPA (Hibernate)
- Spring WebFlux (WebClient)
- Lombok
- MySQL

## Setup Instructions

1. **Configure MySQL**:
   - Ensure a MySQL server is running locally on port `3306`.
   - The application automatically connects to `jdbc:mysql://localhost:3306/defect_inspection_db` and builds/seeds the database on startup using `src/main/resources/schema.sql`.
   - Update `src/main/resources/application.properties` with your custom database username and password if they differ from `root` / `root`.

2. **Build and Run Application**:
   Using Maven, run the following command in the `backend/` folder:
   ```bash
   mvn clean spring-boot:run
   ```
   The backend service starts listening on port `8081`.

## Endpoints Exposed
- `GET /api/products`: Retrieves all registered product boards.
- `POST /api/inspections`: Accepts an inspection file and product ID, invokes AI classification, logs the output, and returns the inspection log.
- `GET /api/inspections`: Retrieves all inspection history (most recent first).
- `GET /api/inspections/stats`: Returns aggregated stats (total, defective count, defect rate, and defect type distribution map).
