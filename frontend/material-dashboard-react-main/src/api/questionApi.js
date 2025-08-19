import axios from "axios";

const BASE_URL = "https://localhost:7181/api/Question";

export const insertQuestion = async (questionData) => {
  return axios.post(`${BASE_URL}/insert`, questionData);
};

export const getQuestionsByCategory = async (categoryId) => {
  return axios.get(`${BASE_URL}/category/${categoryId}`);
};

export const getAllCategories = async () => axios.get(`${BASE_URL}/categories`);
export const fetchQuestions = (categoryId) => axios.get(`${BASE_URL}`, { params: { categoryId } });
export const getQuestionById = (questionId) => axios.get(`${BASE_URL}/${questionId}`);
export const updateQuestion = (data) => axios.put(`${BASE_URL}/update`, data);
export const deleteQuestion = (questionId) => axios.delete(`${BASE_URL}/${questionId}`);
