import React, { useState, useEffect } from 'react';
import { RefreshCw, Cpu, Shield, Lock, LogOut, X, Sun, Moon } from 'lucide-react';
import { fetchInspections, fetchDashboardStats, loginAdmin, deleteInspection, clearInspectionHistory } from './api/inspectionApi';
import StatsCards from './components/StatsCards';
import UploadForm from './components/UploadForm';
import InspectionTable from './components/InspectionTable';
import { Bar } from 'react-chartjs-2';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  Tooltip,
  Legend,
} from 'chart.js';

ChartJS.register(CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend);

function App() {
  const [theme, setTheme] = useState(() => localStorage.getItem('theme') || 'dark');
  const [inspections, setInspections] = useState([]);
  const [stats, setStats] = useState({ totalInspected: 0, defectiveCount: 0, defectRate: 0, defectDistribution: {} });
  const [loading, setLoading] = useState(false);
  const [systemOnline, setSystemOnline] = useState(true);

  // Synchronize document theme attribute
  useEffect(() => {
    document.documentElement.setAttribute('data-theme', theme);
    localStorage.setItem('theme', theme);
  }, [theme]);

  const toggleTheme = () => {
    setTheme(prev => (prev === 'dark' ? 'light' : 'dark'));
  };

  // Admin Portal state & handlers
  const [isAdmin, setIsAdmin] = useState(localStorage.getItem('adminToken') !== null);
  const [adminToken, setAdminToken] = useState(localStorage.getItem('adminToken'));
  const [showLoginModal, setShowLoginModal] = useState(false);
  const [adminUsername, setAdminUsername] = useState('');
  const [adminPassword, setAdminPassword] = useState('');
  const [loginError, setLoginError] = useState('');
  const [authLoading, setAuthLoading] = useState(false);

  const handleAdminLogin = async (e) => {
    e.preventDefault();
    setLoginError('');
    setAuthLoading(true);
    try {
      const data = await loginAdmin(adminUsername, adminPassword);
      localStorage.setItem('adminToken', data.token);
      setAdminToken(data.token);
      setIsAdmin(true);
      setShowLoginModal(false);
      setAdminUsername('');
      setAdminPassword('');
    } catch (err) {
      setLoginError(err.message || 'Login failed. Invalid credentials.');
    } finally {
      setAuthLoading(false);
    }
  };

  const handleAdminLogout = () => {
    localStorage.removeItem('adminToken');
    setAdminToken(null);
    setIsAdmin(false);
  };

  const handleDeleteInspection = async (id) => {
    if (!window.confirm(`Are you sure you want to delete inspection #${id}?`)) {
      return;
    }
    try {
      await deleteInspection(id, adminToken);
      loadData();
    } catch (err) {
      alert(err.message || 'Failed to delete inspection.');
    }
  };

  const handleClearHistory = async () => {
    if (!window.confirm('WARNING: Are you sure you want to delete ALL inspection history? This action cannot be undone.')) {
      return;
    }
    try {
      await clearInspectionHistory(adminToken);
      loadData();
    } catch (err) {
      alert(err.message || 'Failed to clear history.');
    }
  };

  const loadData = async () => {
    setLoading(true);
    try {
      const insData = await fetchInspections();
      setInspections(insData);
      
      const statsData = await fetchDashboardStats();
      setStats(statsData);
      
      setSystemOnline(true);
    } catch (err) {
      console.error('Failed to load data:', err);
      setSystemOnline(false);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  const handleInspectionComplete = () => {
    loadData();
  };

  const distData = stats.defectDistribution || {};
  const labels = Object.keys(distData).map(k => k.replace('_', ' '));
  const dataValues = Object.values(distData);

  const isDarkMode = theme === 'dark';
  const barBg = isDarkMode ? 'rgba(255, 69, 58, 0.75)' : 'rgba(255, 59, 48, 0.75)';
  const barBorder = isDarkMode ? '#ff453a' : '#ff3b30';
  const gridColor = isDarkMode ? 'rgba(255, 255, 255, 0.08)' : 'rgba(0, 0, 0, 0.06)';
  const textColor = isDarkMode ? '#86868b' : '#6e6e73';
  const titleColor = isDarkMode ? '#f5f5f7' : '#1d1d1f';

  const chartData = {
    labels: labels.length > 0 ? labels : ['No Defects'],
    datasets: [
      {
        label: 'Defect Counts',
        data: dataValues.length > 0 ? dataValues : [0],
        backgroundColor: barBg,
        borderColor: barBorder,
        borderWidth: 1,
        borderRadius: 8,
      },
    ],
  };

  const chartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        display: false,
      },
      title: {
        display: true,
        text: 'Defect Type Classification',
        color: titleColor,
        font: {
          family: '-apple-system, BlinkMacSystemFont, SF Pro Display, Inter, sans-serif',
          size: 14,
          weight: '600',
        },
      },
    },
    scales: {
      x: {
        grid: {
          color: gridColor,
        },
        ticks: {
          color: textColor,
          font: {
            family: '-apple-system, BlinkMacSystemFont, SF Pro Display, Inter, sans-serif',
          }
        },
      },
      y: {
        grid: {
          color: gridColor,
        },
        ticks: {
          color: textColor,
          stepSize: 1,
          font: {
            family: '-apple-system, BlinkMacSystemFont, SF Pro Display, Inter, sans-serif',
          }
        },
      },
    },
  };

  return (
    <div className="app-container">
      {/* Header */}
      <header className="header">
        <div className="brand">
          <div className="brand-icon-wrapper">
            <Cpu className="brand-logo" />
          </div>
          <div>
            <h1 className="brand-title">PCB Defect Inspection System</h1>
            <p className="brand-sub">Automated Quality Control powered by YOLOv8 & Deep Learning</p>
          </div>
        </div>
        
        <div className="header-status">
          <span className={`status-badge ${systemOnline ? 'status-badge-online' : 'status-badge-offline'}`}>
            <span style={{ width: '7px', height: '7px', borderRadius: '50%', background: systemOnline ? 'var(--success-green)' : 'var(--danger-red)', display: 'inline-block' }}></span>
            {systemOnline ? 'YOLOv8 Engine Online' : 'Backend Offline'}
          </span>

          <button onClick={toggleTheme} className="theme-toggle-btn" title="Toggle Light / Dark Mode">
            {isDarkMode ? (
              <>
                <Sun className="theme-icon" />
                <span>Light</span>
              </>
            ) : (
              <>
                <Moon className="theme-icon" />
                <span>Dark</span>
              </>
            )}
          </button>
          
          {isAdmin ? (
            <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
              <span className="admin-badge">
                <Shield className="badge-icon" />
                Admin
              </span>
              <button onClick={handleAdminLogout} className="btn-admin btn-admin-logout" title="Exit Admin Mode">
                <LogOut className="badge-icon" />
                <span>Logout</span>
              </button>
            </div>
          ) : (
            <button onClick={() => setShowLoginModal(true)} className="btn-admin" title="Login as Admin">
              <Lock className="badge-icon" />
              <span>Admin Login</span>
            </button>
          )}

          <button onClick={loadData} disabled={loading} className="btn-refresh" title="Refresh Dashboard">
            <RefreshCw className={`refresh-icon-spin ${loading ? 'animate-spin' : ''}`} style={{ width: '16px', height: '16px' }} />
          </button>
        </div>
      </header>

      {/* Metrics Row */}
      <section className="metrics-section">
        <StatsCards stats={stats} />
      </section>

      {/* Left/Right Configuration Grid */}
      <div className="dashboard-row">
        {/* Left Side: Upload Board Form */}
        <div className="dashboard-col flex-40">
          <UploadForm onInspectionComplete={handleInspectionComplete} />
        </div>

        {/* Right Side: Graph Analytics */}
        <div className="dashboard-col flex-60">
          <div className="chart-container">
            <h2 className="card-title">Defect Distribution Analytics</h2>
            <div className="chart-wrapper">
              <Bar data={chartData} options={chartOptions} />
            </div>
          </div>
        </div>
      </div>

      {/* History Data Table Section */}
      <section className="history-container">
        <InspectionTable 
          inspections={inspections} 
          isAdmin={isAdmin}
          onDeleteInspection={handleDeleteInspection}
          onClearHistory={handleClearHistory}
        />
      </section>

      {/* Admin Login Modal */}
      {showLoginModal && (
        <div className="modal-overlay" onClick={() => { setShowLoginModal(false); setLoginError(''); }}>
          <div className="modal-content" style={{ maxWidth: '420px' }} onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
                <Shield style={{ height: '22px', width: '22px', color: 'var(--accent-apple)' }} />
                <h3>Admin Authentication</h3>
              </div>
              <button className="modal-close" onClick={() => { setShowLoginModal(false); setLoginError(''); }}>
                <X className="close-icon" />
              </button>
            </div>
            <form onSubmit={handleAdminLogin}>
              <div className="modal-body" style={{ flexDirection: 'column', gap: '16px', display: 'flex' }}>
                {loginError && (
                  <div className="alert alert-error" style={{ width: '100%', marginBottom: 0 }}>
                    {loginError}
                  </div>
                )}
                
                <div className="form-group" style={{ width: '100%', marginBottom: 0 }}>
                  <label className="form-label">Username</label>
                  <input
                    type="text"
                    required
                    value={adminUsername}
                    onChange={(e) => setAdminUsername(e.target.value)}
                    className="form-input"
                    placeholder="Enter admin username"
                  />
                </div>

                <div className="form-group" style={{ width: '100%', marginBottom: 0 }}>
                  <label className="form-label">Password</label>
                  <input
                    type="password"
                    required
                    value={adminPassword}
                    onChange={(e) => setAdminPassword(e.target.value)}
                    className="form-input"
                    placeholder="Enter admin password"
                  />
                </div>

                <button type="submit" disabled={authLoading} className="btn-submit" style={{ marginTop: '8px' }}>
                  {authLoading ? 'Verifying...' : 'Login to Admin'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}

export default App;

