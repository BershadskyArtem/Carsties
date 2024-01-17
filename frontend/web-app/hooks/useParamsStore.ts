import { create } from "zustand";

type State = {
    pageNumber : number;
    pageSize : number;
    pageCount : number;
    searchTerm : string;
    searchValue : string;
    orderBy : string;
    filterBy : string;
    auction : Auction;
}

type Auction = {
    seller? : string;
    winner? : string;
}


type Actions = {
    setParams: (params: Partial<State>) => void;
    reset : () => void;
    setSearchValue : (value : string) => void;
    setFilterBy : (value : string) => void;
}

const initialState : State = {
    pageNumber : 1,
    pageSize : 4,
    pageCount : 1,
    searchTerm : '',
    searchValue: '',
    orderBy : 'make',
    filterBy : '',
    auction : {
        seller : undefined,
        winner : undefined
    }
}

/*
    create return factory method to create state of type <State & Actions>
    then call that and configure that to have ...initialState
    and we setParams so we can mutate ...initialState.
*/ 

export const useParamsStore = create<State & Actions>()(
    (set) => {
        return {
            ...initialState,
            setParams : (newParams : Partial<State>) => {
                set((state) => {
                    if (newParams.pageNumber){
                        return {...state, pageNumber : newParams.pageNumber}
                    } else {
                        return {...state, ...newParams, pageNumber : 1}
                    }
                        
                });
            },
            reset : () => {set(initialState)},
            setSearchValue : (value : string) => {
                set({searchValue : value});
            },
            setFilterBy : (value : string) => {
                set({filterBy : value});
            }
        }
    }
);