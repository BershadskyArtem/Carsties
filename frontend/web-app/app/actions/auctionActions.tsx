'use server'

import { Auction, PagedResult } from "@/types";

export async function GetData(url: string) : Promise<PagedResult<Auction>> {
    
    const res = await fetch(`http://localhost:6001/search${url}&filterBy=finished`);

    if(!res.ok){
      throw new Error("Failed to search for auctions.");
    }
  
    return res.json();
}