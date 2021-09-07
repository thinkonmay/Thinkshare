import {getCookie} from "./cookie.js"

const server = "http://conductor.thinkmay.net/";
const nudejsSv = "http://retardcrap.hopto.org:3000/";

export const Login = `${server}Account/Login`;
export const Register = `${server}Account/Register`;
export const Dashboard = `${nudejsSv}dashboard`;

export const BearerAuth = `${server}User/FetchSlave`;
export const FetchSlave = `${server}User/FetchSlave`;
export const FetchSession = `${server}User/FetchSession`;

export const genHeaders = token => {
    return {
    	Authorization: `Bearer ${token}`
    }
}