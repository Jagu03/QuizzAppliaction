import AppBar from '@mui/material/AppBar'
import Toolbar from '@mui/material/Toolbar'
import Typography from '@mui/material/Typography'
import Button from '@mui/material/Button'
import Stack from '@mui/material/Stack'
import { Link as RouterLink, useLocation } from 'react-router-dom'

export default function NavBar() {
  const loc = useLocation()
  return (
    <AppBar position="static" color="primary">
      <Toolbar sx={{ display:'flex', justifyContent:'space-between' }}>
        <Typography variant="h6">Impres Quiz</Typography>
        <Stack direction="row" spacing={1}>
          <Button color="inherit" component={RouterLink} to="/">Home</Button>
          <Button color="inherit" component={RouterLink} to="/join">Join</Button>
          <Button color="inherit" component={RouterLink} to="/host">Host</Button>
        </Stack>
      </Toolbar>
    </AppBar>
  )
}