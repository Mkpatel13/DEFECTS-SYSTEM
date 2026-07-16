package com.defectinspection.dto;

import lombok.Data;

@Data
public class AiPredictionResponse {
    private String defectType;
    private Double confidence;
    private Boolean isDefective;
    private String detectedImagePath;
}
