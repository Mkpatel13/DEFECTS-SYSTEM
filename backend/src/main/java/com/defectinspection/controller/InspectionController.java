package com.defectinspection.controller;

import com.defectinspection.dto.DashboardStats;
import com.defectinspection.entity.Inspection;
import com.defectinspection.entity.Product;
import com.defectinspection.repository.ProductRepository;
import com.defectinspection.service.InspectionService;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.multipart.MultipartFile;
import java.util.List;

@RestController
@RequestMapping("/api")
@CrossOrigin(origins = "*") // Enable CORS for React frontend connection
public class InspectionController {

    private final InspectionService inspectionService;
    private final ProductRepository productRepository;

    public InspectionController(InspectionService inspectionService, ProductRepository productRepository) {
        this.inspectionService = inspectionService;
        this.productRepository = productRepository;
    }

    @PostMapping("/inspections")
    public ResponseEntity<?> createInspection(@RequestParam("productId") Long productId,
                                              @RequestParam("file") MultipartFile file) {
        if (file.isEmpty()) {
            return ResponseEntity.status(HttpStatus.BAD_REQUEST).body("File cannot be empty");
        }
        try {
            Inspection inspection = inspectionService.runInspection(productId, file);
            return ResponseEntity.status(HttpStatus.CREATED).body(inspection);
        } catch (IllegalArgumentException e) {
            return ResponseEntity.status(HttpStatus.NOT_FOUND).body(e.getMessage());
        } catch (Exception e) {
            return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR)
                    .body("Failed to complete inspection: " + e.getMessage());
        }
    }

    @GetMapping("/inspections")
    public ResponseEntity<List<Inspection>> getAllInspections() {
        return ResponseEntity.ok(inspectionService.getAllInspections());
    }

    @GetMapping("/inspections/stats")
    public ResponseEntity<DashboardStats> getDashboardStats() {
        return ResponseEntity.ok(inspectionService.getDashboardStats());
    }

    @GetMapping("/products")
    public ResponseEntity<List<Product>> getAllProducts() {
        return ResponseEntity.ok(productRepository.findAll());
    }
}
