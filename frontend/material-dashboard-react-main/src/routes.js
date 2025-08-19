// Material Dashboard 2 React layouts
import Dashboard from "layouts/dashboard";
import Landing from "layouts/landing";
import Profile from "layouts/profile";
import SignIn from "layouts/authentication/sign-in";
import SignUp from "layouts/authentication/sign-up";
import QuestionFormPage from "layouts/question";
import CategoryQuestionsPage from "layouts/categoryQuestions";
import CategoryCreatePage from "layouts/categoryCreate";
import QuestionManagerPage from "layouts/questionManagerPage";
import GroupCreatePage from "layouts/groupCreate";
import AssignmentPublishPage from "layouts/assignmentPublish";
import StudentAssignmentsPage from "layouts/studentAssignments";
import TakeQuizPage from "layouts/takeQuiz";

// @mui icons
import Icon from "@mui/material/Icon";
import QuizIcon from "@mui/icons-material/Quiz";
import CategoryIcon from "@mui/icons-material/Category";

const routes = [
  {
    type: "collapse",
    name: "Landing",
    key: "landing",
    icon: <Icon fontSize="small">home</Icon>,
    route: "/",
    component: <Landing />,
    noSidenav: true, // ✅ hide sidebar
  },
  {
    type: "collapse",
    name: "Dashboard",
    key: "dashboard",
    icon: <Icon fontSize="small">dashboard</Icon>,
    route: "/dashboard",
    component: <Dashboard />,
    noSidenav: false, // ✅ show sidebar
  },
  {
    type: "collapse",
    name: "Profile",
    key: "profile",
    icon: <Icon fontSize="small">person</Icon>,
    route: "/profile",
    component: <Profile />,
    noSidenav: false, // ✅ show sidebar
  },
  {
    type: "collapse",
    name: "Create Category",
    key: "create-category",
    icon: <CategoryIcon fontSize="small" />,
    route: "/create-category",
    component: <CategoryCreatePage />,
    noSidenav: false,
  },
  {
    type: "collapse",
    name: "Create Question",
    key: "question",
    icon: <QuizIcon fontSize="small" />,
    route: "/question",
    component: <QuestionFormPage />,
    noSidenav: false,
  },
  {
    type: "collapse",
    name: "Category Questions",
    key: "category-questions",
    icon: <Icon fontSize="small">list</Icon>,
    route: "/category-questions",
    component: <CategoryQuestionsPage />,
    noSidenav: false,
  },
  {
    type: "collapse",
    name: "Question Manager",
    key: "question-manager",
    icon: <Icon fontSize="small">list</Icon>,
    route: "/question-manager",
    component: <QuestionManagerPage />,
    noSidenav: false,
  },
  {
    type: "collapse",
    name: "Sign In",
    key: "sign-in",
    icon: <Icon fontSize="small">login</Icon>,
    route: "/authentication/sign-in",
    component: <SignIn />,
    noSidenav: true, // ✅ hide sidebar
  },
  {
    type: "collapse",
    name: "Sign Up",
    key: "sign-up",
    icon: <Icon fontSize="small">assignment</Icon>,
    route: "/authentication/sign-up",
    component: <SignUp />,
    noSidenav: true, // ✅ hide sidebar
  },
  {
    type: "collapse",
    name: "Create Group",
    key: "group-create",
    icon: <Icon fontSize="small">groups</Icon>,
    route: "/group-create",
    component: <GroupCreatePage />,
    noSidenav: false,
  },
  {
    type: "collapse",
    name: "Publish Assignment",
    key: "assignment-publish",
    icon: <Icon fontSize="small">assignment</Icon>,
    route: "/assignment-publish",
    component: <AssignmentPublishPage />,
    noSidenav: false,
  },
  {
    type: "collapse",
    name: "My Quizzes",
    key: "student-assignments",
    icon: <Icon fontSize="small">list_alt</Icon>,
    route: "/student-assignments",
    component: <StudentAssignmentsPage />,
    noSidenav: false,
  },
  {
    type: "collapse",
    name: "Take Quiz",
    key: "take-quiz",
    icon: <Icon fontSize="small">quiz</Icon>,
    route: "/take-quiz",
    component: <TakeQuizPage />,
    noSidenav: false, // typically opened via navigate with state
  },
];

export default routes;
