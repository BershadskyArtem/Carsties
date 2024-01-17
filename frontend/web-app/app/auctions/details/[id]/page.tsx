import { GetDetailedAuction } from '@/app/actions/auctionActions'
import Heading from '@/app/components/Heading';
import React from 'react'
import CountdownTimer from '../../CountdownTimer';
import CardImage from '../../CardImage';
import DetailedSpecs from './DetailedSpecs';
import { getCurrentUser } from '@/app/actions/authActions';
import EditButton from './EditButton';
import DeleteButton from './DeleteButton';

export default async function Details({params} : { params : {id: string}}) {

  const data = await GetDetailedAuction(params.id);
  const user = await getCurrentUser();

  return (

    <div>
      <div className='flex justify-between'>
        
        <div className='flex gap-4 items-center'>
          <Heading title={`${data.make} ${data.model}`} />
          {user?.username === data.seller && (
            <>
              <EditButton id={data.id} />
              <DeleteButton id={data.id} />
            </>
            
          )}
        </div>

        <div className='flex gap-3 items-center'>
          <h3 className='text-sl font-semibold'>Time remaining.</h3>
          <CountdownTimer auctionEnd={data.auctionEnd} />
        </div>
      </div>
    
      <div className='grid grid-cols-2 gap-6 mt-3'>
        <div className='w-full bg-gray-200 aspect-h-10 aspect-w-16 rounded-lg overflow-hidden'>
          <CardImage auction={data}/>
        </div>

        <div className='border-2 rounded-lg p-2 bg-gray-100'>
          <Heading title='Bids'/>
        </div>
      </div>

      <div className='mt-3 grid grid-cols-1 rounded-lg'>
        <DetailedSpecs auction={data}/>
      </div>

    </div>

  )
}
