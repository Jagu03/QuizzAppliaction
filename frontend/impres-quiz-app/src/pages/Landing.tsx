import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import Typography from '@mui/material/Typography'
import Stack from '@mui/material/Stack'
import { Link as RouterLink } from 'react-router-dom'

export default function Landing() {
  return (
    <Box textAlign="center" mt={6}>
      <Typography variant="h4" gutterBottom>Welcome to Impres Quiz</Typography>
      <Typography variant="body1" gutterBottom>Choose a mode to continue</Typography>
      <Stack direction={{ xs:'column', sm:'row' }} spacing={2} justifyContent="center" mt={3}>
        <Button variant="contained" size="large" component={RouterLink} to="/join">I am a Player</Button>
        <Button variant="outlined" size="large" component={RouterLink} to="/host">I am a Host</Button>
      </Stack>
    </Box>
  )
}