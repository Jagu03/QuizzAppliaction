// src/api/categoryApi.js

import axios from "axios";

const BASE_URL = "https://localhost:7181/api/Category";

export const fetchCategories = async () => axios.get(`${BASE_URL}/fetch`);
export const mergeCategory = async (data) => axios.post(`${BASE_URL}/merge`, data);
export const deleteCategory = async (id) => axios.delete(`${BASE_URL}/delete/${id}`);
