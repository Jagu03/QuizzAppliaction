import { useEffect, useState } from 'react'
import { getQuizzes } from '@api/endpoints'
import { Link as RouterLink } from 'react-router-dom'
import Box from '@mui/material/Box'
import Typography from '@mui/material/Typography'
import List from '@mui/material/List'
import ListItem from '@mui/material/ListItem'
import ListItemText from '@mui/material/ListItemText'
import Button from '@mui/material/Button'

export default function QuizList() {
  const hostUserId = Number(localStorage.getItem('hostUserId') || 0)
  const [rows, setRows] = useState<any[]>([])

  useEffect(()=>{ (async()=>{
    const data = await getQuizzes(hostUserId, undefined)
    setRows(data)
  })() }, [hostUserId])

  return (
    <Box>
      <Typography variant="h5" gutterBottom>Quizzes</Typography>
      <List>
        {rows.map(q => (
          <ListItem key={q.quizId}
            secondaryAction={<Button component={RouterLink} to={`/host/quizzes/${q.quizId}`}>Edit</Button>}>
            <ListItemText primary={q.title} secondary={q.isPublished ? 'Published' : 'Draft'} />
          </ListItem>
        ))}
      </List>
    </Box>
  )
}