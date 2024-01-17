'use client'

import { Button, TextInput } from 'flowbite-react';
import React, { useEffect } from 'react'
import { FieldValues, useForm } from 'react-hook-form'
import Input from '../components/Input';
import DateInput from '../components/DateInput';
import { CreateAuction, UpdateAuction } from '../actions/auctionActions';
import { usePathname, useRouter } from 'next/navigation';
import toast from 'react-hot-toast';
import { Auction } from '@/types';

type Props = {
    auction? : Auction;
}


export default function Auctionform({auction} : Props) {
    const {
        control,
        register,
        handleSubmit,
        setFocus, 
        reset,
        formState: {
            isSubmitting, isValid, isDirty, errors
        }
    } = useForm({mode: 'onTouched'});

    const pathName = usePathname();
    

    useEffect(() => {
        if (auction) {
            const {make, model, color, mileage, year} = auction;
            reset({make, model, color, mileage, year});
        }
        setFocus('make');
    }, [setFocus]);

    const router = useRouter();

    async function OnSubmit(data : FieldValues) {
        try {
            let id = '';
            let response;
            if(pathName === '/auctions/create'){
                response = await CreateAuction(data);    
                id = response.id;
            } else {
                if(auction){
                    response = await UpdateAuction(data, auction.id);
                    id = auction.id;
                }
            }

            if (response.error){
                throw response.error;
            }

            router.push(`/auctions/details/${id}`);
        } catch (error : any) {
            toast.error(error.status + error.message);
        }
    }

    return (
        <form className='flex flex-col m-3' onSubmit={handleSubmit(OnSubmit)}>
            <Input label='Make' name='make' control={control} rules={{required : 'Make is required.'}}/>
            <Input label='Model' name='model' control={control} rules={{required : 'Model is required.'}}/>
            <Input label='Color' name='color' control={control} rules={{required : 'Color is required.'}}/>

            <div className='grid grid-cols-2 gap-3'>
                <Input label='Year' name='year' type='number' control={control} rules={{required : 'Year is required.'}}/>
                <Input label='Mileage' name='mileage' type='number' control={control} rules={{required : 'Mileage is required.'}}/>
            </div>

            {
                pathName === '/auctions/create' && 
                <>
                    <Input label='Image URL' name='imageUrl' control={control} rules={{required : 'Image is required.'}}/>

                    <div className='grid grid-cols-2 gap-3'>
                        <Input label='Reserve price (enter 0 if no reserve)' name='reservePrice' type='number' control={control} rules={{required : 'Reserve price is required.'}}/>
                        <DateInput 
                            dateFormat='dd MMMM yyyy h:mm a' 
                            showTimeSelect 
                            control={control} 
                            label={'Auction end date/time'} 
                            name={'auctionEnd'} 
                            rules={{required : 'Image is required.'}}/>
                    </div>
                </>

            }

            <div className='flex justify-between'>
                <Button outline color='gray' onClick={() => {
                    router.back()
                }}>Cancel</Button>
                <Button 
                    isProcessing={isSubmitting} 
                    outline 
                    color='success'
                    type='submit'
                    disabled={!isValid}>Submit</Button>
            </div>

        </form>
    )
}
