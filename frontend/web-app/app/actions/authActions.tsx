import { getServerSession } from "next-auth";
import { AuthOptions } from "../api/auth/[...nextauth]/route";

export async function getSession(){
    return await getServerSession(AuthOptions);
}

export async function getCurrentUser() : 
Promise<{
    name?: string | null | undefined;
    email?: string | null | undefined;
    image?: string | null | undefined;} | undefined | null> 
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