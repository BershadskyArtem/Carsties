'use client'
import React from 'react'
import { useParamsStore } from '@/hooks/useParamsStore';
import { Button, ButtonGroup } from 'flowbite-react';
import { AiOutlineClockCircle, AiOutlineSortAscending } from 'react-icons/ai';
import { BsFillStopCircleFill, BsStopwatchFill } from 'react-icons/bs'
import {GiFlame, GiFinishLine} from 'react-icons/gi'

const pageSizeButtons = [4, 8, 12];

const orderButtons = [
    {
        label : 'Alphabetical',
        icon : AiOutlineSortAscending,
        value : 'make'
    },
    {
        label : 'End date',
        icon : AiOutlineClockCircle,
        value : 'endingSoon'
    },
    {
        label : 'Recently added',
        icon : BsFillStopCircleFill,
        value : 'new'
    }
];

const filterButtons = [
    {
        label : 'Finished',
        icon : BsStopwatchFill,
        value : 'finished'
    },
    {
        label : 'Ending soon (< 6 hrs)',
        icon : GiFinishLine,
        value : 'endingSoon'
    },
    {
        label : 'Live',
        icon : GiFlame,
        value : 'ongoing'
    }
];



export default function Filters() {

    const pageSize = useParamsStore(state => state.pageSize);
    const orderBy = useParamsStore(state => state.orderBy);
    const filterBy = useParamsStore(state => state.filterBy);

    const setParams = useParamsStore(state => state.setParams);
    const setPageSize = (pageSize : number) => {
        setParams({pageSize});
    }

    return (
        <div className='flex justify-between items-center mb-4'>
            <div>
                <span className='uppercase text-sm text-gray-500 mr-2'>
                    Order by
                </span>

                <ButtonGroup>
                    {orderButtons.map( ({value, label, icon : Icon}) => {
                        return (
                            <Button 
                                className='border focus:ring-0'
                                color={`${orderBy === value ? 'red' : 'grey'}`}
                                onClick={() => setParams({orderBy: value})}
                                key={value}>
                                <Icon className='mr-3 h-4 w-4'/>
                                {label}
                            </Button>
                        );
                    })}
                </ButtonGroup>

            </div>


            <div>
                <span className='uppercase text-sm text-gray-500 mr-2'>
                    Filter by
                </span>

                <ButtonGroup>
                    {filterButtons.map( ({value, label, icon : Icon}) => {
                        return (
                            <Button 
                                className='border focus:ring-0'
                                color={`${filterBy === value ? 'red' : 'grey'}`}
                                onClick={() => setParams({filterBy: value})}
                                key={value}>
                                <Icon className='mr-3 h-4 w-4'/>
                                {label}
                            </Button>
                        );
                    })}
                </ButtonGroup>
            </div>


            <div>
                <span className='uppercase text-sm text-gray-500 mr-2'>
                    Page size
                </span>
                <ButtonGroup>
                    {pageSizeButtons.map((value, index) => {
                        return (
                            <Button 
                                className='border focus:ring-0'
                                key={index} 
                                onClick={() => {
                                    setPageSize(value);
                                }}
                                color={`${pageSize === value ? 'red' : 'grey'}`}
                                >
                                {value}
                            </Button>
                        );
                    })}
                </ButtonGroup>

            </div>
        </div>
    )
}
