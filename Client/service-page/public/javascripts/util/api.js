import {getCookie} from "./cookie.js"

const server = "http://conductor.thinkmay.net";

export const Login = `${server}/Account/Login`;
export const LoginAdmin = `${server}/Account/Login-admin`;
export const Register = `${server}/Account/Register`;
export const Dashboard = `/dashboard`;
export const DashboardAdmin = `/dashboard-admin`;

export const BearerAuth = `${server}/User/FetchSlave`;
export const FetchSlave = `${server}/User/FetchSlave`;
export const FetchSession = `${server}/User/FetchSession`;

export const Initialize = `/initialize`;

export const genHeaders = token => {
    return {
    	Authorization: `Bearer ${token}`
    }
}