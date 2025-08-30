import { useState } from 'react'
import Box from '@mui/material/Box'
import TextField from '@mui/material/TextField'
import Button from '@mui/material/Button'
import Typography from '@mui/material/Typography'
import Alert from '@mui/material/Alert'
import Stack from '@mui/material/Stack'
import { getSessionByPin, joinByPin } from '@api/endpoints'
import { useNavigate } from 'react-router-dom'

export default function PlayerJoin() {
  const [pin, setPin] = useState('')
  const [name, setName] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)
  const nav = useNavigate()

  const onCheck = async () => {
    setError('')
    try {
      const s = await getSessionByPin(pin)
      if (!s) setError('No active session for this PIN')
    } catch (e: any) {
      setError('Invalid PIN')
    }
  }

  const onJoin = async () => {
    setLoading(true)
    setError('')
    try {
      const res = await joinByPin(pin, name || 'Player')
      localStorage.setItem('playerId', String(res.playerId))
      localStorage.setItem('sessionId', res.gameSessionId)
      nav(`/play/${res.gameSessionId}`)
    } catch (e: any) {
      const msg =
        e?.response?.data?.message ||
        e?.response?.data?.error ||
        e?.message || 'Unable to join. Check PIN or try again.'
      setError(msg)
      console.error('join error', e?.response || e)
    } finally {
      setLoading(false)
    }
  }


  return (
    <Box maxWidth={420} mx="auto" mt={4}>
      <Typography variant="h5" gutterBottom>Join a Game</Typography>
      <Stack spacing={2}>
        <TextField label="PIN" value={pin} onChange={e => setPin(e.target.value)} inputProps={{ maxLength: 6 }} />
        <TextField label="Display Name" value={name} onChange={e => setName(e.target.value)} />
        <Stack direction="row" spacing={1}>
          <Button variant="outlined" onClick={onCheck}>Check PIN</Button>
          <Button variant="contained" onClick={onJoin} disabled={!pin || loading}>Join</Button>
        </Stack>
        {error && <Alert severity="error">{error}</Alert>}
      </Stack>
    </Box>
  )
}