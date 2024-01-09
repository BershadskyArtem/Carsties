'use client'

import React, { useEffect, useState } from 'react'
import queryString from 'query-string';
import { shallow } from 'zustand/shallow';


import AppPagination from '../components/AppPagination';
import AuctionCard from './AuctionCard';
import Filters from './Filters';

import { Auction, PagedResult } from '@/types';

import { GetData } from '../actions/auctionActions';

import { useParamsStore } from '@/hooks/useParamsStore';
import EmptyFilter from '../components/EmptyFilter';


// REMOVE ASYNC. THIS SHIT HURTS.

export default function Listings() {
    
    const [data, setData] = useState<PagedResult<Auction>>();

    // Zustand devs can eat my ass with that deprication.
    // It is deprecated but you can use this https://github.com/pmndrs/zustand/discussions/1937#discussioncomment-6963038 instead)
    const params = useParamsStore(state => ({
        pageNumber : state.pageNumber,
        pageSize : state.pageSize,
        searchTerm : state.searchTerm,
        orderBy : state.orderBy,
        filterBy : state.filterBy
    }), shallow);

    const setParams = useParamsStore(state => state.setParams);

    function setPageNumber(pageNumber : number) : void {
        setParams({pageNumber});
    }

    const url = queryString.stringifyUrl({
        url: '',
        query: params
    });

    // This is gonna be executed when the components first loads and then updates on state change.
    // If dependencies is empty then this effect will run only once. 
    // if something inside dependencies change then it will rerun and rerender component.
    useEffect(() => {
        GetData(url).then(data => {
            setData(data);
        });
    }, [url]);

    if(!data){
        return (
            <h3>Loading...</h3>
        );
    }
        
    return (
        <>
            <Filters/>

            {data.results.length === 0 ? (
                <EmptyFilter showReset={true}/>
            ) : (
                <>
                    <div className='grid lg:grid-cols-4 gap-6 md:grid-cols-2'>
                        {data.results && data.results.map((auction)=> (
                            <AuctionCard auction={auction} key={auction.id}/>
                        ))}
                    </div>
                    <div className='flex justify-center mt-4'>
                        <AppPagination
                            pageChanged={setPageNumber}
                            currentPage={params.pageNumber}
                            pageCount={data.pageCount}
                        />
                    </div>
                </>    
            )}
        </>
    )
}
