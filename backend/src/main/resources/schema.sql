-- Create Products Table
CREATE TABLE IF NOT EXISTS products (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    sku VARCHAR(100) UNIQUE NOT NULL,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create Inspections Table
CREATE TABLE IF NOT EXISTS inspections (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    product_id BIGINT NOT NULL,
    image_path VARCHAR(500),
    is_defective BOOLEAN NOT NULL,
    defect_type VARCHAR(100),
    confidence DOUBLE,
    inspected_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE
);

-- Seed Sample Products (if they don't already exist)
INSERT IGNORE INTO products (id, sku, name, description) VALUES 
(1, 'PCB-REV-A', 'Main Controller Board Rev A', 'Microcontroller-based main system control board for assembly line testing.'),
(2, 'PCB-REV-B', 'Power Supply Board Rev B', 'High voltage transformer board with regulators and rectifiers.'),
(3, 'PCB-REV-C', 'RF Transceiver Board Rev C', 'High-frequency wireless communication board.');
