import { useEffect, useMemo, useRef, useState } from 'react'
import { useParams } from 'react-router-dom'
import Box from '@mui/material/Box'
import Typography from '@mui/material/Typography'
import Card from '@mui/material/Card'
import CardContent from '@mui/material/CardContent'
import Button from '@mui/material/Button'
import Stack from '@mui/material/Stack'
import LinearProgress from '@mui/material/LinearProgress'
import Alert from '@mui/material/Alert'
import { getSessionStatus, getQuestionsWithChoices, submitAnswer } from '@api/endpoints'

export default function PlayerPlay() {
  const { sessionId } = useParams()
  const playerId = Number(localStorage.getItem('playerId') || 0)
  const [quizId, setQuizId] = useState<number | null>(null)
  const [questions, setQuestions] = useState<any[]>([])
  const [qi, setQi] = useState(0)
  const [selectedChoice, setSelectedChoice] = useState<number | null>(null)
  const [timeLeft, setTimeLeft] = useState<number>(0)
  const [error, setError] = useState('')
  const timerRef = useRef<number | null>(null)

  useEffect(()=>{ (async()=>{
    if(!sessionId) return
    const s = await getSessionStatus(sessionId)
    setQuizId(s.quizId)
  })() }, [sessionId])

  useEffect(()=>{ (async()=>{
    if(!quizId) return
    const qs = await getQuestionsWithChoices(quizId)
    setQuestions(qs)
    if(qs.length > 0) setTimeLeft(qs[0].timeLimitSec)
  })() }, [quizId])

  useEffect(()=>{
    if (questions.length === 0) return
    const limit = questions[qi]?.timeLimitSec ?? 20
    setTimeLeft(limit)
    if (timerRef.current) window.clearInterval(timerRef.current)
    timerRef.current = window.setInterval(()=>{
      setTimeLeft(prev => {
        if (prev <= 1) { window.clearInterval(timerRef.current!); return 0 }
        return prev - 1
      })
    }, 1000) as unknown as number
    return ()=>{ if (timerRef.current) window.clearInterval(timerRef.current) }
  }, [qi, questions])

  const current = questions[qi]
  const onSubmit = async (choiceId:number) => {
    if (!sessionId || !playerId || !current) return
    try {
      const elapsed = (current.timeLimitSec - timeLeft) * 1000
      await submitAnswer(sessionId, playerId, current.questionId, choiceId, Math.max(0, elapsed))
      setSelectedChoice(choiceId)
    } catch (e:any) {
      setError('Submit failed. Maybe already answered?')
    }
  }

  const next = () => {
    setSelectedChoice(null)
    setQi(q => Math.min(q + 1, questions.length - 1))
  }

  if(!current) return <Alert sx={{mt:2}} severity="info">Loading questions...</Alert>

  return (
    <Box mt={2}>
      <Typography variant="h6">Q{qi+1}. {current.text}</Typography>
      <LinearProgress variant="determinate" value={(timeLeft / current.timeLimitSec) * 100} sx={{ my: 1 }} />
      <Stack spacing={1}>
        {current.choices.map((c:any)=> (
          <Card key={c.choiceId} variant="outlined">
            <CardContent>
              <Button fullWidth variant={selectedChoice===c.choiceId ? 'contained' : 'outlined'}
                onClick={()=>onSubmit(c.choiceId)} disabled={selectedChoice !== null || timeLeft===0}>
                {c.text}
              </Button>
            </CardContent>
          </Card>
        ))}
      </Stack>
      <Stack direction="row" spacing={2} mt={2}>
        <Button onClick={next} variant="contained" disabled={qi>=questions.length-1}>Next</Button>
      </Stack>
      {error && <Alert sx={{mt:2}} severity="error">{error}</Alert>}
    </Box>
  )
}