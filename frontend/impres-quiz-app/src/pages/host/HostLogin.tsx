import { useState } from 'react'
import Box from '@mui/material/Box'
import TextField from '@mui/material/TextField'
import Button from '@mui/material/Button'
import Typography from '@mui/material/Typography'
import Alert from '@mui/material/Alert'

/**
 * Dev-only stub: we don't have JWT yet. 
 * Enter an existing Host UserId to simulate login.
 */
export default function HostLogin() {
  const [userId, setUserId] = useState('1')
  const [error, setError] = useState('')

  const onSubmit = (e:React.FormEvent) => {
    e.preventDefault()
    if (!/^[0-9]+$/.test(userId)) { setError('Enter numeric UserId'); return }
    localStorage.setItem('hostUserId', userId)
    window.location.href = '/host'
  }

  return (
    <Box component="form" onSubmit={onSubmit} maxWidth={400} mx="auto" mt={4}>
      <Typography variant="h5" gutterBottom>Host Login (Dev)</Typography>
      <Typography variant="body2" color="text.secondary" gutterBottom>
        Temporary: enter your UserId to continue. Replace with JWT later.
      </Typography>
      <TextField fullWidth label="UserId" value={userId} onChange={e=>setUserId(e.target.value)} />
      {error && <Alert sx={{mt:2}} severity="error">{error}</Alert>}
      <Button sx={{mt:2}} type="submit" variant="contained" fullWidth>Continue</Button>
    </Box>
  )
}