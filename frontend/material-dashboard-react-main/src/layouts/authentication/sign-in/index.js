import React, { useState } from "react";
import { useNavigate } from "react-router-dom";

// @mui material components
import Card from "@mui/material/Card";
import Switch from "@mui/material/Switch";
import Grid from "@mui/material/Grid";
import MuiLink from "@mui/material/Link";
import Alert from "@mui/material/Alert";

// @mui icons
import FacebookIcon from "@mui/icons-material/Facebook";
import GitHubIcon from "@mui/icons-material/GitHub";
import GoogleIcon from "@mui/icons-material/Google";

// Material Dashboard 2 React components
import MDBox from "components/MDBox";
import MDTypography from "components/MDTypography";
import MDInput from "components/MDInput";
import MDButton from "components/MDButton";

// Authentication layout components
import BasicLayout from "layouts/authentication/components/BasicLayout";

// Images
import bgImage from "assets/images/bg-sign-in-basic.jpeg";

function SignIn() {
  const [rememberMe, setRememberMe] = useState(false);
  const [loginCode, setLoginCode] = useState("");
  const [userPassword, setUserPassword] = useState("");
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [touched, setTouched] = useState({ username: false, password: false });
  const navigate = useNavigate();

  const handleSetRememberMe = () => setRememberMe(!rememberMe);

  const handleLogin = async (e) => {
    e.preventDefault();
    setError("");
    setSuccess("");

    if (!loginCode || !userPassword) return;

    try {
      const response = await fetch("https://localhost:7181/api/Auth/login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          LoginCode: loginCode,
          UserPassword: userPassword,
          BVersion: "1.0.0",
        }),
      });

      const data = await response.json();

      if (!response.ok || (data.result !== 1 && data.result !== 3)) {
        setError(data.message || "Login failed");
        return;
      }

      // Save user details
      localStorage.setItem("token", data.token);
      localStorage.setItem(
        "user",
        JSON.stringify({
          welcomeName: data.welcomeName,
          lastLogin: data.lastLogin,
          image: data.image,
        })
      );

      // Show success and redirect after 2 seconds
      setSuccess("✅ Login Successful! Redirecting to Dashboard...");
      setTimeout(() => {
        navigate("/dashboard");
      }, 2000);
    } catch (err) {
      console.error(err);
      setError("Backend error: could not connect to server");
    }
  };

  return (
    <BasicLayout image={bgImage}>
      <Card>
        <MDBox
          variant="gradient"
          bgColor="info"
          borderRadius="lg"
          coloredShadow="info"
          mx={2}
          mt={-3}
          p={2}
          mb={1}
          textAlign="center"
        >
          <MDTypography variant="h4" fontWeight="medium" color="white" mt={1}>
            Staff Login
          </MDTypography>
          <Grid container spacing={3} justifyContent="center" sx={{ mt: 1, mb: 2 }}>
            <Grid item xs={2}>
              <MDTypography component={MuiLink} href="#" variant="body1" color="white">
                <FacebookIcon color="inherit" />
              </MDTypography>
            </Grid>
            <Grid item xs={2}>
              <MDTypography component={MuiLink} href="#" variant="body1" color="white">
                <GitHubIcon color="inherit" />
              </MDTypography>
            </Grid>
            <Grid item xs={2}>
              <MDTypography component={MuiLink} href="#" variant="body1" color="white">
                <GoogleIcon color="inherit" />
              </MDTypography>
            </Grid>
          </Grid>
        </MDBox>

        <MDBox pt={4} pb={3} px={3}>
          <form onSubmit={handleLogin}>
            <MDBox mb={2}>
              <MDInput
                type="text"
                label="Username"
                fullWidth
                value={loginCode}
                onChange={(e) => setLoginCode(e.target.value)}
                onBlur={() => setTouched((t) => ({ ...t, username: true }))}
                error={touched.username && !loginCode}
              />
              {touched.username && !loginCode && (
                <MDTypography variant="caption" color="error">
                  Please enter the username
                </MDTypography>
              )}
            </MDBox>
            <MDBox mb={2}>
              <MDInput
                type="password"
                label="Password"
                fullWidth
                value={userPassword}
                onChange={(e) => setUserPassword(e.target.value)}
                onBlur={() => setTouched((t) => ({ ...t, password: true }))}
                error={touched.password && !userPassword}
              />
              {touched.password && !userPassword && (
                <MDTypography variant="caption" color="error">
                  Please enter the password
                </MDTypography>
              )}
            </MDBox>
            <MDBox display="flex" alignItems="center" ml={-1}>
              <Switch checked={rememberMe} onChange={handleSetRememberMe} />
              <MDTypography
                variant="button"
                fontWeight="regular"
                color="text"
                onClick={handleSetRememberMe}
                sx={{ cursor: "pointer", userSelect: "none", ml: -1 }}
              >
                &nbsp;&nbsp;Remember me
              </MDTypography>
            </MDBox>
            <MDBox mt={4} mb={1}>
              <MDButton
                type="submit"
                variant="gradient"
                color="info"
                fullWidth
                disabled={!loginCode || !userPassword}
              >
                Sign In
              </MDButton>
            </MDBox>

            {error && (
              <Alert severity="error" sx={{ mt: 2, fontSize: "1rem", fontWeight: "bold" }}>
                {error}
              </Alert>
            )}
            {success && (
              <Alert severity="success" sx={{ mt: 2, fontSize: "1rem", fontWeight: "bold" }}>
                {success}
              </Alert>
            )}
          </form>
        </MDBox>
      </Card>
    </BasicLayout>
  );
}

export default SignIn;
