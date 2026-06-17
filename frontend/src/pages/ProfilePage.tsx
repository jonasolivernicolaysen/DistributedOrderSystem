import { useState, useEffect } from "react";


// trenger brukernavn, epost, mine produkter, balance
function ProfilePage() {

    interface UserDetails {
        username: string,
        email: string,
        balance: number
    }
    // state
    const [details, setDetails] = useState<UserDetails>();

    // functions
    const getUserDetails = async () => {
        const token = localStorage.getItem("token");

        // send request to authservice getUserDetails endpoint
        const response = await fetch(
            "https://localhost:7144/api/auth/profile",
            {
                method: "GET",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${token}`
                }
            });

        if (!response.ok) {
            alert(`Request failed: ${response.status}`);
            return;
        }
        const data = await response.json();
        setDetails(data)
        console.log(data)
        return data;
    }

    

    useEffect(() => {
        getUserDetails();
    }, [])
    // html
    return (
        <div>
            <p>{details?.username}</p>
            <p>{details?.email}</p>
            <p>{details?.balance}</p>
            <p>My products:</p>
            
        </div>
    );
}


export default ProfilePage;