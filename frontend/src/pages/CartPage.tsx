
function CartPage() {
    // state
    const [products, setProducts] = useState([]);

    // functions

    // get cart items from backend
    const getCartItems = async () => {
        try {
            // get token
            const token = localStorage.getItem("token");

            // send request to authservice getcart endpoint
            const response = await fetch(
                "https://localhost:7144/api/orders/cart",
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

            setProducts(data);

            
            // get all items from the response

        } catch (error) {
            console.error(error);
            alert("Could not connect to the server");
        }
    }

    // return html

}

export default CartPage;