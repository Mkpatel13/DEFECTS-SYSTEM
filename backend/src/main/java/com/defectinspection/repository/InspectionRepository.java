package com.defectinspection.repository;

import com.defectinspection.entity.Inspection;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.stereotype.Repository;
import java.util.List;

@Repository
public interface InspectionRepository extends JpaRepository<Inspection, Long> {
    List<Inspection> findByIsDefective(Boolean isDefective);
    
    @Query("SELECT COUNT(i) FROM Inspection i WHERE i.isDefective = true")
    long countDefective();
    
    List<Inspection> findAllByOrderByIdDesc();
}
