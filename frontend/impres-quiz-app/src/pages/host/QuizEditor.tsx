import { useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'
import { getQuiz, getQuestionsWithChoices } from '@api/endpoints'
import Box from '@mui/material/Box'
import Typography from '@mui/material/Typography'
import Card from '@mui/material/Card'
import CardContent from '@mui/material/CardContent'
import List from '@mui/material/List'
import ListItem from '@mui/material/ListItem'
import ListItemText from '@mui/material/ListItemText'
import Divider from '@mui/material/Divider'

export default function QuizEditor() {
  const { quizId } = useParams()
  const [quiz, setQuiz] = useState<any>(null)
  const [questions, setQuestions] = useState<any[]>([])

  useEffect(()=>{ (async()=>{
    if(!quizId) return
    const [qz, qs] = await Promise.all([getQuiz(Number(quizId)), getQuestionsWithChoices(Number(quizId))])
    setQuiz(qz); setQuestions(qs)
  })() }, [quizId])

  if(!quiz) return null

  return (
    <Box>
      <Typography variant="h5" gutterBottom>{quiz.title}</Typography>
      <Typography variant="body2" color="text.secondary" gutterBottom>{quiz.description}</Typography>

      <Card sx={{ mt:2 }}>
        <CardContent>
          <Typography variant="h6">Questions</Typography>
          <List>
            {questions.map((q:any)=> (
              <>
                <ListItem key={q.questionId} alignItems="flex-start">
                  <ListItemText
                    primary={`${q.orderNo}. ${q.text} (${q.points} pts / ${q.timeLimitSec}s)`}
                    secondary={q.choices.map((c:any)=>`${c.isCorrect ? '✅ ':''}${c.text}`).join('  |  ')}
                  />
                </ListItem>
                <Divider component="li" />
              </>
            ))}
          </List>
        </CardContent>
      </Card>
    </Box>
  )
}