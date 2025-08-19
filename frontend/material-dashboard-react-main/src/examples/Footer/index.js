import PropTypes from "prop-types";

// @mui material components
import Link from "@mui/material/Link";
import Icon from "@mui/material/Icon";

// Material Dashboard 2 React components
import MDBox from "components/MDBox";
import MDTypography from "components/MDTypography";

// Material Dashboard 2 React base styles
import typography from "assets/theme/base/typography";

function Footer({ company, links }) {
  const { href, name } = company;
  const { size } = typography;

  return (
    <MDBox
      width="100%"
      display="flex"
      flexDirection={{ xs: "column", lg: "row" }}
      justifyContent="space-between"
      alignItems="center"
      px={1.5}
    >
      <MDBox
        display="flex"
        justifyContent="center"
        alignItems="center"
        flexWrap="wrap"
        color="text"
        fontSize={size.sm}
        px={1.5}
      >
        &copy; {new Date().getFullYear()}, made with
        <MDBox fontSize={size.md} color="text" mb={-0.5} mx={0.25}>
          <Icon color="inherit" fontSize="inherit">
            favorite
          </Icon>
        </MDBox>
        by
        <Link href={href} target="_blank">
          <MDTypography variant="button" fontWeight="medium">
            &nbsp;{name}&nbsp;
          </MDTypography>
        </Link>
        for a better web.
      </MDBox>

      {/* Hide the links section entirely (kept in case you want to add items later) */}
      {links?.length > 0 && (
        <MDBox
          component="ul"
          sx={({ breakpoints }) => ({
            display: "flex",
            flexWrap: "wrap",
            alignItems: "center",
            justifyContent: "center",
            listStyle: "none",
            mt: 3,
            mb: 0,
            p: 0,
            [breakpoints.up("lg")]: { mt: 0 },
          })}
        >
          {links.map((link) => (
            <MDBox key={link.name} component="li" px={2} lineHeight={1}>
              <Link href={link.href} target="_blank">
                <MDTypography variant="button" fontWeight="regular" color="text">
                  {link.name}
                </MDTypography>
              </Link>
            </MDBox>
          ))}
        </MDBox>
      )}
    </MDBox>
  );
}

// Defaults: show IMPRES ERP and no extra links
Footer.defaultProps = {
  company: { href: "#", name: "IMPRES ERP" },
  links: [], // no Creative Tim / About / Blog / License
};

// Typechecking props for the Footer
Footer.propTypes = {
  company: PropTypes.objectOf(PropTypes.string),
  links: PropTypes.arrayOf(PropTypes.object),
};

export default Footer;
