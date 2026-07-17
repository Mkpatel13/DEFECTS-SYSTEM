import React, { useState } from 'react';
import { ShieldCheck, ShieldAlert, Image, X, Trash2 } from 'lucide-react';

const InspectionTable = ({ inspections, isAdmin, onDeleteInspection, onClearHistory }) => {
  const [activeImage, setActiveImage] = useState(null);

  const formatDate = (dateStr) => {
    if (!dateStr) return '';
    const date = new Date(dateStr);
    return date.toLocaleString();
  };

  return (
    <div className="table-container">
      <div className="table-header-flex">
        <h2 className="card-title">Recent Inspection History</h2>
        {isAdmin && inspections.length > 0 && (
          <button onClick={onClearHistory} className="btn-clear-all" title="Clear all inspection history">
            <Trash2 className="badge-icon" />
            <span>Clear History</span>
          </button>
        )}
      </div>
      {inspections.length === 0 ? (
        <div className="empty-state">No inspections conducted yet.</div>
      ) : (
        <div className="table-responsive">
          <table className="inspection-table">
            <thead>
              <tr>
                <th>ID</th>
                <th>PCB Model</th>
                <th>Inspected At</th>
                <th>Status</th>
                <th>Defect Type</th>
                <th>Confidence</th>
                <th>Visuals</th>
                {isAdmin && <th>Actions</th>}
              </tr>
            </thead>
            <tbody>
              {inspections.map((ins) => (
                <tr key={ins.id}>
                  <td>#{ins.id}</td>
                  <td>
                    <div className="product-info">
                      <span className="product-name">{ins.product.name}</span>
                      <span className="product-sku">{ins.product.sku}</span>
                    </div>
                  </td>
                  <td>{formatDate(ins.inspectedAt)}</td>
                  <td>
                    {ins.isDefective ? (
                      <span className="badge badge-defective">
                        <ShieldAlert className="badge-icon" />
                        Defective
                      </span>
                    ) : (
                      <span className="badge badge-clean">
                        <ShieldCheck className="badge-icon" />
                        Passed
                      </span>
                    )}
                  </td>
                  <td className="defect-type-text">
                    {ins.isDefective ? ins.defectType.replace('_', ' ') : 'None'}
                  </td>
                  <td>
                    {ins.isDefective ? `${(ins.confidence * 100).toFixed(1)}%` : '-'}
                  </td>
                  <td>
                    {ins.imagePath ? (
                      <button
                        onClick={() => setActiveImage(ins.imagePath)}
                        className="btn-view-image"
                        title="View Annotated Bounding Box"
                      >
                        <Image className="view-icon" />
                        <span>View</span>
                      </button>
                    ) : (
                      <span className="text-muted">No Image</span>
                    )}
                  </td>
                  {isAdmin && (
                    <td>
                      <button
                        onClick={() => onDeleteInspection(ins.id)}
                        className="btn-delete"
                        title="Delete Inspection Record"
                      >
                        <Trash2 className="view-icon" />
                        <span>Delete</span>
                      </button>
                    </td>
                  )}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* Image Modal Popup */}
      {activeImage && (
        <div className="modal-overlay" onClick={() => setActiveImage(null)}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h3>Annotated PCB Inspection Bounding Box</h3>
              <button className="modal-close" onClick={() => setActiveImage(null)}>
                <X className="close-icon" />
              </button>
            </div>
            <div className="modal-body">
              <img
                src={
                  activeImage.startsWith("detected_images") 
                    ? `http://localhost:8000/${activeImage}` 
                    : activeImage
                }
                alt="PCB Defect Detail"
                className="modal-image"
                onError={(e) => {
                  e.target.src = 'https://placehold.co/600x400/1e293b/f8fafc?text=Annotated+Image+Not+Found';
                }}
              />
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default InspectionTable;

