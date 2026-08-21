package com.defectinspection.entity;

import jakarta.persistence.*;
import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;
import java.time.LocalDateTime;

@Entity
@Table(name = "inspections")
@Data
@NoArgsConstructor
@AllArgsConstructor
public class Inspection {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    @ManyToOne(fetch = FetchType.EAGER)
    @JoinColumn(name = "product_id", nullable = false)
    private Product product;

    @Column(name = "pcb_id", length = 50)
    private String pcbId;

    @Column(name = "image_path", length = 500)
    private String imagePath;

    @Column(name = "is_defective", nullable = false)
    private Boolean isDefective;

    @Column(name = "defect_type", length = 100)
    private String defectType;

    private Double confidence;

    @Column(name = "inspected_at", insertable = false, updatable = false)
    private LocalDateTime inspectedAt;
}
