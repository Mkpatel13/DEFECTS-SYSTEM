package com.defectinspection.service;

import com.defectinspection.dto.AiPredictionResponse;
import com.defectinspection.dto.DashboardStats;
import com.defectinspection.entity.Inspection;
import com.defectinspection.entity.Product;
import com.defectinspection.repository.InspectionRepository;
import com.defectinspection.repository.ProductRepository;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.core.io.ByteArrayResource;
import org.springframework.http.HttpEntity;
import org.springframework.http.MediaType;
import org.springframework.http.client.MultipartBodyBuilder;
import org.springframework.stereotype.Service;
import org.springframework.util.MultiValueMap;
import org.springframework.web.multipart.MultipartFile;
import org.springframework.web.reactive.function.BodyInserters;
import org.springframework.web.reactive.function.client.WebClient;
import java.io.IOException;
import java.util.List;
import java.util.Map;
import java.util.stream.Collectors;

@Service
public class InspectionService {

    private final ProductRepository productRepository;
    private final InspectionRepository inspectionRepository;
    private final WebClient webClient;

    public InspectionService(ProductRepository productRepository, 
                             InspectionRepository inspectionRepository,
                             WebClient.Builder webClientBuilder,
                             @Value("${ai.service.url}") String aiServiceUrl) {
        this.productRepository = productRepository;
        this.inspectionRepository = inspectionRepository;
        this.webClient = webClientBuilder.baseUrl(aiServiceUrl).build();
    }

    public Inspection runInspection(Long productId, MultipartFile file) throws IOException {
        // 1. Fetch Product
        Product product = productRepository.findById(productId)
                .orElseThrow(() -> new IllegalArgumentException("Product not found with id: " + productId));

        // 2. Prepare multipart body for Python API
        MultipartBodyBuilder bodyBuilder = new MultipartBodyBuilder();
        ByteArrayResource resource = new ByteArrayResource(file.getBytes()) {
            @Override
            public String getFilename() {
                return file.getOriginalFilename();
            }
        };
        bodyBuilder.part("file", resource, MediaType.parseMediaType(file.getContentType()));
        MultiValueMap<String, HttpEntity<?>> multipartBody = bodyBuilder.build();

        // 3. Call AI Service POST /predict
        AiPredictionResponse prediction = webClient.post()
                .uri("/predict")
                .contentType(MediaType.MULTIPART_FORM_DATA)
                .body(BodyInserters.fromMultipartData(multipartBody))
                .retrieve()
                .bodyToMono(AiPredictionResponse.class)
                .block(); 

        if (prediction == null) {
            throw new RuntimeException("AI prediction service returned empty response");
        }

        // 4. Save Inspection to MySQL DB
        Inspection inspection = new Inspection();
        inspection.setProduct(product);
        inspection.setIsDefective(prediction.getIsDefective());
        inspection.setDefectType(prediction.getDefectType());
        inspection.setConfidence(prediction.getConfidence());
        
        String path = prediction.getDetectedImagePath() != null 
                ? prediction.getDetectedImagePath() 
                : "uploaded_images/" + file.getOriginalFilename();
        inspection.setImagePath(path);

        return inspectionRepository.save(inspection);
    }

    public List<Inspection> getAllInspections() {
        return inspectionRepository.findAllByOrderByIdDesc();
    }

    public DashboardStats getDashboardStats() {
        List<Inspection> inspections = inspectionRepository.findAll();
        long total = inspections.size();
        if (total == 0) {
            return new DashboardStats(0, 0, 0.0, Map.of());
        }

        long defectiveCount = inspections.stream().filter(Inspection::getIsDefective).count();
        double defectRate = ((double) defectiveCount / total) * 100.0;

        // Group defective results by type for distribution chart
        Map<String, Long> distribution = inspections.stream()
                .filter(Inspection::getIsDefective)
                .collect(Collectors.groupingBy(Inspection::getDefectType, Collectors.counting()));

        return new DashboardStats(total, defectiveCount, defectRate, distribution);
    }

    public void deleteInspection(Long id) {
        if (!inspectionRepository.existsById(id)) {
            throw new IllegalArgumentException("Inspection not found with id: " + id);
        }
        inspectionRepository.deleteById(id);
    }

    public void deleteAllInspections() {
        inspectionRepository.deleteAll();
    }
}
