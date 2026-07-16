import React from 'react';
import { Eye, ShieldAlert, Percent } from 'lucide-react';

const StatsCards = ({ stats }) => {
  const { totalInspected = 0, defectiveCount = 0, defectRate = 0.0 } = stats || {};

  const cards = [
    {
      title: "Total Inspected",
      value: totalInspected,
      icon: <Eye className="stat-icon text-blue" />,
      className: "stat-card-blue"
    },
    {
      title: "Defective PCBs",
      value: defectiveCount,
      icon: <ShieldAlert className="stat-icon text-rose" />,
      className: "stat-card-rose"
    },
    {
      title: "Defect Rate",
      value: `${defectRate.toFixed(1)}%`,
      icon: <Percent className="stat-icon text-amber" />,
      className: "stat-card-amber"
    }
  ];

  return (
    <div className="stats-container">
      {cards.map((card, index) => (
        <div key={index} className={`stat-card ${card.className}`}>
          <div className="stat-info">
            <h3 className="stat-title">{card.title}</h3>
            <p className="stat-value">{card.value}</p>
          </div>
          <div className="stat-icon-container">
            {card.icon}
          </div>
        </div>
      ))}
    </div>
  );
};

export default StatsCards;
