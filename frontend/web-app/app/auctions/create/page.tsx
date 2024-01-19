import Heading from '@/app/components/Heading'
import React from 'react'
import Auctionform from '../AuctionForm'

export default function Create() {
  return (
    <div className='mx-auto max-w-[75%] shadow-lg p-10 bg-white rounded-lg'>
      <Heading title='Sell your car.' subtitle='Please enter details of your car.' />
      <Auctionform/>
    </div>
  )
}