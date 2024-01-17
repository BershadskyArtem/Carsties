'use server'

import { fetchWrapper } from "@/lib/FetchWrapper";
import { Auction, PagedResult } from "@/types";
import { revalidatePath } from "next/cache";
import { FieldValues } from "react-hook-form";

export async function GetData(url: string) : Promise<PagedResult<Auction>> {
    return await fetchWrapper.Get(`search${url}`);
}

export async function CreateAuction(data : FieldValues){
  return await fetchWrapper.Post('auctions', data);
}

export async function GetDetailedAuction(id : string): Promise<Auction>{
  return await fetchWrapper.Get(`auctions/${id}`);
}

export async function UpdateAuction(data : FieldValues, id : string) {
  const result = await fetchWrapper.Put(`auctions/${id}`, data);
  revalidatePath(`/auctions/${id}`);
  return result;
}

export async function DeleteAuction(id : string){
  return await fetchWrapper.Del(`auctions/${id}`);
}