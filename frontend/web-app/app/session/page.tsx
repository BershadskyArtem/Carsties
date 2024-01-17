import React from 'react'
import { GetTokenAlternative, getSession } from '../actions/authActions'
import Heading from '../components/Heading';

export default async function Session() {
  const session = await getSession();
  const token = await GetTokenAlternative();

  return (
    <div>
      <Heading title='Dashboard'/>
      <div className='bg-blue-200 border-2 border-blue-500'>
        <h3 className='text-lg'>Session data</h3>
        <pre>{JSON.stringify(session, null, 2)}</pre>
      </div>

      <div className='bg-green-200 border-2 border-green-500 mt-4'>
        <h3 className='text-lg'>Token data</h3>
        <pre className='overflow-auto' >{JSON.stringify(token, null, 2)}</pre>
      </div>

    </div>
  )
}

