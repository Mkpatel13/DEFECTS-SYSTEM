package com.defectinspection.dto;

import lombok.Data;

@Data
public class InspectionResultDto {
    private Long productId;
    private String pcbId;
    private Boolean isDefective;
    private String defectType;
    private Double confidence;
    private String imagePath;
}
