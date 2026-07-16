package com.defectinspection.dto;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;
import java.util.Map;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class DashboardStats {
    private long totalInspected;
    private long defectiveCount;
    private double defectRate;
    private Map<String, Long> defectDistribution;
}
