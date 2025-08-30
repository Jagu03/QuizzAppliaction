import { useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'
import { getLeaderboard } from '@api/endpoints'
import Box from '@mui/material/Box'
import Typography from '@mui/material/Typography'
import List from '@mui/material/List'
import ListItem from '@mui/material/ListItem'
import ListItemText from '@mui/material/ListItemText'
import Chip from '@mui/material/Chip'

export default function Leaderboard() {
  const { sessionId } = useParams()
  const [rows, setRows] = useState<any[]>([])

  useEffect(()=>{ (async()=>{
    if (!sessionId) return
    const data = await getLeaderboard(sessionId)
    setRows(data)
  })() }, [sessionId])

  return (
    <Box mt={2}>
      <Typography variant="h5" gutterBottom>Leaderboard</Typography>
      <List>
        {rows.map((r, idx)=> (
          <ListItem key={r.playerId}
            secondaryAction={<Chip label={`${r.totalScore} pts`} color={idx===0?'success':'default'} />}>
            <ListItemText primary={`${idx+1}. ${r.displayName}`} secondary={`✅ ${r.correctCount}  |  Answered ${r.answeredCount}`} />
          </ListItem>
        ))}
      </List>
    </Box>
  )
}