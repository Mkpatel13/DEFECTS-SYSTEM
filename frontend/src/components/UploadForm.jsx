import React, { useState, useEffect } from 'react';
import { Upload, AlertCircle, CheckCircle, RefreshCw } from 'lucide-react';
import { fetchProducts, submitInspection } from '../api/inspectionApi';

const UploadForm = ({ onInspectionComplete }) => {
  const [products, setProducts] = useState([]);
  const [selectedProductId, setSelectedProductId] = useState('');
  const [selectedFile, setSelectedFile] = useState(null);
  const [previewUrl, setPreviewUrl] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  useEffect(() => {
    const loadProducts = async () => {
      try {
        const data = await fetchProducts();
        setProducts(data);
        if (data.length > 0) {
          setSelectedProductId(data[0].id.toString());
        }
      } catch (err) {
        setError('Failed to load product configurations.');
      }
    };
    loadProducts();
  }, []);

  const handleFileChange = (e) => {
    const file = e.target.files[0];
    if (file) {
      if (!file.type.startsWith('image/')) {
        setError('Please upload a valid image file.');
        return;
      }
      setSelectedFile(file);
      setPreviewUrl(URL.createObjectURL(file));
      setError('');
      setSuccess('');
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!selectedProductId) {
      setError('Please select a product model.');
      return;
    }
    if (!selectedFile) {
      setError('Please select an image to inspect.');
      return;
    }

    setLoading(true);
    setError('');
    setSuccess('');

    try {
      const result = await submitInspection(selectedProductId, selectedFile);
      setSuccess(`Success: ${result.isDefective ? 'Defect Detected (' + result.defectType + ')' : 'Board is clean.'}`);
      setSelectedFile(null);
      setPreviewUrl('');
      const fileInput = document.getElementById('pcb-file-input');
      if (fileInput) fileInput.value = '';
      
      onInspectionComplete(result);
    } catch (err) {
      setError(err.message || 'An error occurred during inspection.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="upload-container">
      <h2 className="card-title">New PCB Inspection</h2>
      <form onSubmit={handleSubmit}>
        {error && (
          <div className="alert alert-error">
            <AlertCircle className="alert-icon" />
            <span>{error}</span>
          </div>
        )}
        {success && (
          <div className="alert alert-success">
            <CheckCircle className="alert-icon" />
            <span>{success}</span>
          </div>
        )}

        <div className="form-group">
          <label className="form-label" htmlFor="product-select">Select PCB Model</label>
          <select
            id="product-select"
            className="form-select"
            value={selectedProductId}
            onChange={(e) => setSelectedProductId(e.target.value)}
            disabled={loading}
          >
            {products.map((prod) => (
              <option key={prod.id} value={prod.id}>
                {prod.name} ({prod.sku})
              </option>
            ))}
          </select>
        </div>

        <div className="form-group">
          <label className="form-label">PCB Image Upload</label>
          <div className="dropzone">
            <input
              id="pcb-file-input"
              type="file"
              accept="image/*"
              onChange={handleFileChange}
              disabled={loading}
              className="dropzone-input"
            />
            <label htmlFor="pcb-file-input" className="dropzone-label">
              <Upload className="upload-icon-style" />
              <p className="dropzone-text">Click to choose or drag an image here</p>
              <p className="dropzone-hint">Supports PNG, JPG, JPEG</p>
            </label>
          </div>
        </div>

        {previewUrl && (
          <div className="preview-container">
            <p className="preview-header">Image Selected:</p>
            <div className="preview-box">
              <img src={previewUrl} alt="PCB Preview" className="preview-img" />
            </div>
          </div>
        )}

        <button type="submit" disabled={loading} className="btn-submit">
          {loading ? (
            <>
              <RefreshCw className="spinner animate-spin" />
              <span>Analyzing PCB Board...</span>
            </>
          ) : (
            <span>Run YOLOv8 Inspection</span>
          )}
        </button>
      </form>
    </div>
  );
};

export default UploadForm;
