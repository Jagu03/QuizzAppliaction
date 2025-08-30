import { useEffect, useState } from 'react'
import Box from '@mui/material/Box'
import Typography from '@mui/material/Typography'
import Button from '@mui/material/Button'
import TextField from '@mui/material/TextField'
import Grid from '@mui/material/Grid'
import Card from '@mui/material/Card'
import CardContent from '@mui/material/CardContent'
import CardActions from '@mui/material/CardActions'
import List from '@mui/material/List'
import ListItem from '@mui/material/ListItem'
import ListItemText from '@mui/material/ListItemText'
import Stack from '@mui/material/Stack'
import Alert from '@mui/material/Alert'
import { createSession, getPlayers, getQuizzes, startSession, endSession, cancelSession, kickPlayer } from '@api/endpoints'
import type { Guid } from '@api/endpoints'
import { Link as RouterLink } from 'react-router-dom'

export default function HostDashboard() {
  const hostUserId = Number(localStorage.getItem('hostUserId') || 0)
  const [quizzes, setQuizzes] = useState<any[]>([])
  const [selectedQuizId, setSelectedQuizId] = useState<number | null>(null)
  const [sessionId, setSessionId] = useState<Guid | null>(null)
  const [pin, setPin] = useState<string>('')
  const [players, setPlayers] = useState<any[]>([])
  const [status, setStatus] = useState<'idle'|'lobby'|'running'|'ended'|'cancelled'>('idle')
  const [error, setError] = useState<string>('')

  useEffect(() => {
    (async () => {
      try {
        const list = await getQuizzes(hostUserId, true) // published for simplicity
        setQuizzes(list)
      } catch (e:any) {
        setError(e.message)
      }
    })()
  }, [hostUserId])

  const onCreateSession = async () => {
    if (!selectedQuizId) return
    const res = await createSession(selectedQuizId, hostUserId)
    setSessionId(res.gameSessionId)
    setPin(res.pinCode)
    setStatus('lobby')
    setPlayers([])
  }

  const onRefreshPlayers = async () => {
    if (!sessionId) return
    const list = await getPlayers(sessionId)
    setPlayers(list)
  }

  const onStart = async () => {
    if (!sessionId) return
    await startSession(sessionId)
    setStatus('running')
  }
  const onEnd = async () => {
    if (!sessionId) return
    await endSession(sessionId)
    setStatus('ended')
  }
  const onCancel = async () => {
    if (!sessionId) return
    await cancelSession(sessionId)
    setStatus('cancelled')
  }

  if (!hostUserId) {
    return (
      <Alert severity="warning" sx={{mt:2}}>
        No host logged in. Please <RouterLink to="/host/login">login</RouterLink>.
      </Alert>
    )
  }

  return (
    <Box mt={2}>
      <Typography variant="h5" gutterBottom>Host Dashboard</Typography>
      {error && <Alert severity="error" sx={{mb:2}}>{error}</Alert>}

      <Grid container spacing={2}>
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6">1) Choose a Quiz</Typography>
              <TextField
                select
                SelectProps={{ native: true }}
                fullWidth
                label="Quiz"
                value={selectedQuizId ?? ''}
                onChange={(e)=>setSelectedQuizId(Number(e.target.value)||null)}
                sx={{ mt: 2 }}
              >
                <option value="">-- select --</option>
                {quizzes.map(q => <option key={q.quizId} value={q.quizId}>{q.title}</option>)}
              </TextField>
            </CardContent>
            <CardActions>
              <Button onClick={onCreateSession} variant="contained" disabled={!selectedQuizId}>Create Session</Button>
              <Button component={RouterLink} to="/host/quizzes" color="secondary">Manage Quizzes</Button>
            </CardActions>
          </Card>
        </Grid>

        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6">2) Session</Typography>
              <Stack spacing={1} mt={1}>
                <TextField label="Session Id" value={sessionId ?? ''} InputProps={{ readOnly: true }} />
                <TextField label="PIN" value={pin} InputProps={{ readOnly: true }} />
                <Typography variant="body2">Ask players to Join with PIN.</Typography>
              </Stack>
            </CardContent>
            <CardActions>
              <Button onClick={onStart} variant="contained" disabled={!sessionId || status!=='lobby'}>Start</Button>
              <Button onClick={onEnd} color="warning" disabled={!sessionId || status!=='running'}>End</Button>
              <Button onClick={onCancel} color="error" disabled={!sessionId || status==='cancelled' || status==='ended'}>Cancel</Button>
              <Button onClick={onRefreshPlayers} variant="outlined" disabled={!sessionId}>Refresh Players</Button>
            </CardActions>
          </Card>
        </Grid>

        <Grid item xs={12}>
          <Card>
            <CardContent>
              <Typography variant="h6">Players</Typography>
              <List dense>
                {players.map(p => (
                  <ListItem key={p.playerId}
                    secondaryAction={<Button size="small" color="error" onClick={()=>kickPlayer(p.playerId, sessionId!)}>Kick</Button>}
                  >
                    <ListItemText primary={p.displayName} secondary={new Date(p.joinedAt).toLocaleString()} />
                  </ListItem>
                ))}
              </List>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  )
}