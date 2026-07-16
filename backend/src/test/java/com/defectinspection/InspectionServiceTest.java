package com.defectinspection;

import com.defectinspection.dto.AiPredictionResponse;
import com.defectinspection.entity.Inspection;
import com.defectinspection.entity.Product;
import com.defectinspection.repository.InspectionRepository;
import com.defectinspection.repository.ProductRepository;
import com.defectinspection.service.InspectionService;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;
import org.springframework.mock.web.MockMultipartFile;
import org.springframework.web.reactive.function.client.WebClient;
import java.io.IOException;
import java.util.Optional;
import static org.junit.jupiter.api.Assertions.*;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.*;

@ExtendWith(MockitoExtension.class)
public class InspectionServiceTest {

    @Mock
    private ProductRepository productRepository;

    @Mock
    private InspectionRepository inspectionRepository;

    @Mock
    private WebClient.Builder webClientBuilder;

    @Mock
    private WebClient webClient;

    private InspectionService inspectionService;

    @BeforeEach
    public void setUp() {
        when(webClientBuilder.baseUrl(anyString())).thenReturn(webClientBuilder);
        when(webClientBuilder.build()).thenReturn(webClient);
        
        inspectionService = new InspectionService(
                productRepository,
                inspectionRepository,
                webClientBuilder,
                "http://localhost:8000"
        );
    }

    @Test
    public void testRunInspection_ProductNotFound() {
        when(productRepository.findById(1L)).thenReturn(Optional.empty());
        MockMultipartFile file = new MockMultipartFile("file", "test.jpg", "image/jpeg", new byte[]{1, 2, 3});

        Exception exception = assertThrows(IllegalArgumentException.class, () -> {
            inspectionService.runInspection(1L, file);
        });

        assertEquals("Product not found with id: 1", exception.getMessage());
        verify(inspectionRepository, never()).save(any());
    }
}
