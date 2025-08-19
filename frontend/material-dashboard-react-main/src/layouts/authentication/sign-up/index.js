import React, { useState } from "react";
import axios from "axios";
import { useNavigate, Link } from "react-router-dom";

// @mui material components
import Card from "@mui/material/Card";
import Checkbox from "@mui/material/Checkbox";

// Material Dashboard 2 React components
import MDBox from "components/MDBox";
import MDTypography from "components/MDTypography";
import MDInput from "components/MDInput";
import MDButton from "components/MDButton";

// Authentication layout components
import CoverLayout from "layouts/authentication/components/CoverLayout";

// Background Image
import bgImage from "assets/images/bg-sign-up-cover.jpeg";

function StudentLogin() {
  const [rollNo, setRollNo] = useState("");
  const [dob, setDob] = useState("");
  const [password, setPassword] = useState("");
  const [rememberMe, setRememberMe] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const navigate = useNavigate();

  const handleSetRememberMe = () => setRememberMe(!rememberMe);

  const handleLogin = async (e) => {
    e.preventDefault();
    setError("");
    setSuccess("");

    const trimmedRollNo = rollNo.trim();
    const trimmedPassword = password.trim();
    const trimmedDob = dob.trim();
    const loginAs = 1; // 1 = Student Login

    // Encode password in Base64 for student
    const encodedPassword = loginAs === 1 ? btoa(trimmedPassword) : trimmedPassword;

    try {
      const response = await axios.post("https://localhost:7181/api/StudentAuth/login", {
        RollNo: trimmedRollNo,
        StudPassword: encodedPassword,
        Dob: trimmedDob,
        LoginAs: loginAs,
        IPAddr: "127.0.0.1",
        SessId: "session-abc",
        Browser: navigator.userAgent,
        BVersion: navigator.appVersion,
        OtherLoginId: 0,
        DeviceId: "device-id-123",
      });

      if (response.data && response.data.result === 1) {
        setSuccess("Login successful!");
        localStorage.setItem("token", response.data.token);

        // Redirect to dashboard
        navigate("/dashboard");
      } else {
        setError(response.data.message || "Invalid login");
      }
    } catch (err) {
      console.error("Login error details:", err);
      if (err.response && err.response.status === 401) {
        setError(err.response.data.message || "Invalid credentials");
      } else if (err.response) {
        setError(err.response.data.message || "Error: " + err.response.status);
      } else if (err.request) {
        setError("No response from server. Check connection or CORS.");
      } else {
        setError("Something went wrong. " + err.message);
      }
    }
  };

  return (
    <CoverLayout image={bgImage}>
      <Card>
        {/* Header */}
        <MDBox
          variant="gradient"
          bgColor="info"
          borderRadius="lg"
          coloredShadow="info"
          mx={2}
          mt={-3}
          p={3}
          mb={1}
          textAlign="center"
        >
          <MDTypography variant="h4" fontWeight="medium" color="white" mt={1}>
            Student Login
          </MDTypography>
          <MDTypography display="block" variant="button" color="white" my={1}>
            Please enter your Roll No, DOB, and Password
          </MDTypography>
        </MDBox>

        {/* Form */}
        <MDBox pt={4} pb={3} px={3}>
          <form onSubmit={handleLogin}>
            <MDBox mb={2}>
              <MDInput
                type="text"
                label="Roll No"
                variant="standard"
                fullWidth
                value={rollNo}
                onChange={(e) => setRollNo(e.target.value)}
                required
              />
            </MDBox>
            <MDBox mb={2}>
              <MDInput
                type="text"
                label="Date of Birth (dd/mm/yyyy)"
                variant="standard"
                fullWidth
                value={dob}
                onChange={(e) => setDob(e.target.value)}
                required
              />
            </MDBox>
            <MDBox mb={2}>
              <MDInput
                type="password"
                label="Password"
                variant="standard"
                fullWidth
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
              />
            </MDBox>
            <MDBox display="flex" alignItems="center" ml={-1}>
              <Checkbox checked={rememberMe} onChange={handleSetRememberMe} />
              <MDTypography
                variant="button"
                fontWeight="regular"
                color="text"
                sx={{ cursor: "pointer", userSelect: "none", ml: -1 }}
                onClick={handleSetRememberMe}
              >
                &nbsp;&nbsp;Remember me
              </MDTypography>
            </MDBox>
            <MDBox mt={4} mb={1}>
              <MDButton type="submit" variant="gradient" color="info" fullWidth>
                Login
              </MDButton>
            </MDBox>

            {/* Error & Success Messages (Prettier Fixed) */}
            {error && (
              <MDTypography variant="caption" color="error" display="block" textAlign="center">
                {error}
              </MDTypography>
            )}
            {success && (
              <MDTypography variant="caption" color="success" display="block" textAlign="center">
                {success}
              </MDTypography>
            )}

            {/* <MDBox mt={3} mb={1} textAlign="center">
              <MDTypography variant="button" color="text">
                Are you Staff?{" "}
                <MDTypography
                  component={Link}
                  to="/authentication/sign-in"
                  variant="button"
                  color="info"
                  fontWeight="medium"
                  textGradient
                >
                  Staff Login
                </MDTypography>
              </MDTypography>
            </MDBox> */}
          </form>
        </MDBox>
      </Card>
    </CoverLayout>
  );
}

export default StudentLogin;
