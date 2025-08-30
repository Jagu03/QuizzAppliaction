import { Routes, Route, Navigate } from 'react-router-dom'
import CssBaseline from '@mui/material/CssBaseline'
import Container from '@mui/material/Container'
import Box from '@mui/material/Box'
import { ThemeProvider, createTheme } from '@mui/material/styles'
import NavBar from '@components/NavBar'

import Landing from '@pages/Landing'
import HostLogin from '@pages/host/HostLogin'
import HostDashboard from '@pages/host/HostDashboard'
import PlayerJoin from '@pages/player/PlayerJoin'
import PlayerPlay from '@pages/player/PlayerPlay'
import Leaderboard from '@pages/player/Leaderboard'
import QuizList from '@pages/host/QuizList'
import QuizEditor from '@pages/host/QuizEditor'

const theme = createTheme()

export default function App() {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <NavBar />
      <Container maxWidth="md">
        <Box py={2}>
          <Routes>
            <Route path="/" element={<Landing />} />

            {/* Host */}
            <Route path="/host/login" element={<HostLogin />} />
            <Route path="/host" element={<HostDashboard />} />
            <Route path="/host/quizzes" element={<QuizList />} />
            <Route path="/host/quizzes/:quizId" element={<QuizEditor />} />

            {/* Player */}
            <Route path="/join" element={<PlayerJoin />} />
            <Route path="/play/:sessionId" element={<PlayerPlay />} />
            <Route path="/leaderboard/:sessionId" element={<Leaderboard />} />

            <Route path="*" element={<Navigate to="/" />} />
          </Routes>
        </Box>
      </Container>
    </ThemeProvider>
  )
}