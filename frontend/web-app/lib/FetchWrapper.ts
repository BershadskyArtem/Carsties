import { GetTokenAlternative } from "@/app/actions/authActions";
import { getToken } from "next-auth/jwt";

const baseUrl = 'http://localhost:6001/'

async function Get(url : string){
    const requestOptions : RequestInit = {
        method: 'GET',
        headers : await GetHeaders()
    }

    const response = await fetch(baseUrl + url, requestOptions);

    return await ProcessResponseAsync(response);
}

async function Post(url: string, body : any) {
    const requestOptions : RequestInit = {
        method : 'POST',
        headers : await GetHeaders(),
        body : JSON.stringify(body)
    }

    const response = await fetch(baseUrl + url, requestOptions);
    return await ProcessResponseAsync(response);
}

async function Put(url: string, body : any) {
    const requestOptions : RequestInit = {
        method : 'PUT',
        headers : await GetHeaders(),
        body : JSON.stringify(body)
    }

    const response = await fetch(baseUrl + url, requestOptions);
    return await ProcessResponseAsync(response);
}

async function Del(url: string) {
    const requestOptions : RequestInit = {
        method : 'DELETE',
        headers : await GetHeaders()
    }

    const response = await fetch(baseUrl + url, requestOptions);
    return await ProcessResponseAsync(response);
}


async function GetHeaders() {
    const token = await GetTokenAlternative();
    const headers = {'Content-type' : 'application/json'} as any;
    if (token) {
        headers.Authorization = 'Bearer ' + token.access_token;
    }

    return headers;
}


async function ProcessResponseAsync(response: Response) {
    const text = await response.text();
    const data = text && JSON.parse(text);

    if (response.ok){
        return data || response.statusText;
    } else {
        const error = {
            status : response.status,
            message : response.statusText
        }
        return {error};
    }
}

export const fetchWrapper = {
    Get,
    Post,
    Put,
    Del
}


