import { http } from './http'

export type Guid = string

// --- DTOs mirrored from your backend ---
export interface CreateGameSessionResponse { gameSessionId: Guid; pinCode: string }
export interface JoinPlayerResponse { playerId: number; gameSessionId: Guid }
export interface LeaderboardRow {
  playerId: number; displayName: string; totalScore: number;
  correctCount: number; answeredCount: number; joinedAt: string
}
export interface SessionStatusDto {
  gameSessionId: Guid; quizId: number; hostUserId: number;
  pinCode: string; status: number; startedAt?: string; endedAt?: string; createdAt: string
}

export interface ChoiceDto { choiceId: number; text: string; isCorrect: boolean; orderNo: number }
export interface QuestionDto {
  questionId: number; quizId: number; text: string; questionType: number;
  timeLimitSec: number; points: number; mediaUrl?: string; orderNo: number; choices: ChoiceDto[]
}

export async function createSession(quizId:number, hostUserId:number) {
  const res = await http.post<CreateGameSessionResponse>('/api/game-sessions', { quizId, hostUserId })
  return res.data
}
export async function startSession(id:Guid) { await http.post(`/api/game-sessions/${id}/start`, {}) }
export async function endSession(id:Guid) { await http.post(`/api/game-sessions/${id}/end`, {}) }
export async function cancelSession(id:Guid) { await http.post(`/api/game-sessions/${id}/cancel`, {}) }

export async function joinByPin(pinCode:string, displayName:string) {
  const res = await http.post<JoinPlayerResponse>('/api/players/join', { pinCode, displayName })
  return res.data
}
export async function submitAnswer(gameSessionId:Guid, playerId:number, questionId:number, choiceId:number, timeTakenMs:number) {
  await http.post('/api/answers', { gameSessionId, playerId, questionId, choiceId, timeTakenMs })
}

export async function getLeaderboard(sessionId:Guid) {
  const res = await http.get<LeaderboardRow[]>(`/api/leaderboard/${sessionId}`)
  return res.data
}
export async function getSessionStatus(id:Guid) {
  const res = await http.get<SessionStatusDto>(`/api/sessions/${id}/status`)
  return res.data
}
export async function getSessionByPin(pin:string) {
  const res = await http.get<SessionStatusDto>(`/api/sessions/by-pin/${pin}/status`)
  return res.data
}
export async function getPlayers(id:Guid) {
  const res = await http.get<{playerId:number;displayName:string;isKicked:boolean;joinedAt:string}[]>(`/api/sessions/${id}/players`)
  return res.data
}
export async function kickPlayer(playerId:number, gameSessionId:Guid) {
  await http.post(`/api/players/${playerId}/kick`, { gameSessionId })
}
export async function unkickPlayer(playerId:number, gameSessionId:Guid) {
  await http.post(`/api/players/${playerId}/unkick`, { gameSessionId })
}

// Content authoring GETs
export async function getQuizzes(createdBy?:number, isPublished?:boolean) {
  const params:any = {}
  if (createdBy !== undefined) params.createdBy = createdBy
  if (isPublished !== undefined) params.isPublished = isPublished
  const res = await http.get(`/api/admin/quizzes`, { params })
  return res.data as { quizId:number; title:string; isPublished:boolean; createdBy:number; createdAt:string; updatedAt?:string }[]
}
export async function getQuiz(quizId:number) {
  const res = await http.get(`/api/admin/quizzes/${quizId}`)
  return res.data as { quizId:number; title:string; description?:string; isPublished:boolean; createdBy:number; createdAt:string; updatedAt?:string; questionCount:number }
}
export async function getQuestionsWithChoices(quizId:number) {
  const res = await http.get<QuestionDto[]>(`/api/admin/quizzes/${quizId}/questions`)
  return res.data
}