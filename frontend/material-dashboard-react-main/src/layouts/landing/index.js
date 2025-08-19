import React from "react";
import { useNavigate } from "react-router-dom";
import { Typography, Button } from "@mui/material";
import PersonIcon from "@mui/icons-material/Person";
import AdminPanelSettingsIcon from "@mui/icons-material/AdminPanelSettings";
import "./Landing.css";

function Landing() {
  const navigate = useNavigate();

  return (
    <div className="landing-container">
      <div className="landing-card">
        {/* Welcome Text */}
        <Typography variant="h4" gutterBottom className="landing-title">
          Welcome to <span className="highlight">IMPRES KAHOOT</span>
        </Typography>
        <Typography variant="body2" className="landing-subtitle">
          Please select your login type and verify your identity to continue.
        </Typography>

        {/* Buttons */}
        <div className="landing-buttons">
          <Button
            variant="contained"
            className="student-btn"
            startIcon={<AdminPanelSettingsIcon />}
            onClick={() => navigate("/authentication/sign-in")} // already working route
          >
            STAFF
          </Button>
          <Button
            variant="outlined"
            className="admin-btn"
            startIcon={<PersonIcon />}
            onClick={() => navigate("/authentication/sign-up")} // already working route
          >
            STUDENT
          </Button>
        </div>
      </div>
    </div>
  );
}

export default Landing;
