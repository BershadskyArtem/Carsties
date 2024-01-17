import { getServerSession } from "next-auth";
import { AuthOptions } from "../api/auth/[...nextauth]/route";
import { cookies, headers } from 'next/headers'
import { NextApiRequest } from "next";
import { getToken } from "next-auth/jwt";

export async function getSession(){
    return await getServerSession(AuthOptions);
}

export async function getCurrentUser() : 
Promise<{
    id : string;
    name?: string | null | undefined;
    email?: string | null | undefined;
    image?: string | null | undefined; 
    username : string } | undefined | null> 
{
    try {
        const session = await getSession();

        console.log(session);
        
        if(!session){
            return null;
        }

        return session.user;

    } catch (error) {
        return null;
    }
}

/*
    The hell is this shit?!! JS/NextJS/insert your shiny new thing here JS is abomination of ass.
*/
export async function GetTokenAlternative(){
    const req = {
        headers : Object.fromEntries(headers() as Headers),
        cookies : Object.fromEntries(cookies().getAll().map(c => [c.name, c.value]))
    } as NextApiRequest;

    return await getToken({req});
}