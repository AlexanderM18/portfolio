import axios from "axios";
import debug from "debug"
import { onGlobalError, onGlobalSuccess, API_HOST_PREFIX } from "./serviceHelpers"

const _logger = debug.extend("userService")

const users = { userUrl: `${API_HOST_PREFIX}/api/users` }
const tempAuth = { url: `${API_HOST_PREFIX}/api/temp/auth` }

let register = (payload) => {
    _logger("register middle")
    const config = {
        method: "POST",
        url: users.userUrl,
        data: payload,
        withCredentials: true,
        crossdomain: true,
        headers: { "Content-Type": "application/json" }
    };
    return axios(config).then(onGlobalSuccess).catch(onGlobalError);
};

let login = (payload) => {
    _logger("login middle")
    const config = {
        method: "POST",
        url: `${users.userUrl}/login`,
        data: payload,
        withCredentials: true,
        crossdomain: true,
        headers: { "Content-Type": "application/json" }
    };
    return axios(config).then(onGlobalSuccess).catch(onGlobalError);
};

let logout = () => {
    _logger("logout middle")
    const config = {
        method: "GET",
        url: `${tempAuth.url}/logout`,
        withCredentials: true,
        crossdomain: true,
        headers: { "Content-Type": "application/json" }
    };
    return axios(config).then(onGlobalSuccess).catch(onGlobalError);
};

export { register, login, logout }
