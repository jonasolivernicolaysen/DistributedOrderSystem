import { useEffect, useState } from "react"; 
import { useNavigate } from "react-router-dom";

interface CartItem {
    productId: string,
    name: string,
    description: string,
    unitPrice: number,
    quantity: number
}

function CartPage() {
    // state

    const [products, setProducts] = useState<CartItem[]>([]);
    const navigate = useNavigate();

    // functions   

    const checkout = async () => {
        try {
            // get token
            const token = localStorage.getItem("token");

            // send request to authservice getcart endpoint
            const response = await fetch(
                "https://localhost:7144/api/orders/cart/checkout",
                {
                    method: "POST",
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
            navigate(`/payments/${data.paymentId}`)
            return data;

            // fetches the items instead of the cart 
        } catch (error) {
            console.error(error);
            alert("Could not connect to the server");
        }
    }


    const goToProductsPage = () => {
        navigate("/products");
    }

    const totalPrice = products?.reduce(
        (sum, item) => sum + item.unitPrice * item.quantity, 0
    ) ?? 0;


    useEffect(() => {
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

                // fetches the items instead of the cart 
                setProducts(data.items);

            } catch (error) {
                console.error(error);
                alert("Could not connect to the server");
            }
        }

        getCartItems();
    }, [])

    // return html
    return (
        <div>
            <h1>Cart</h1>

            {products.length > 0 && (
                <div className="row fw-bold border-bottom pb-2 mb-3">
                    <div className="col-md-4">Name</div>
                    <div className="col-md-2">Price</div>
                    <div className="col-md-2">Quantity</div>
                    <div className="col-md-4 text-end">Sum</div>
                </div>
            )}
            
            <div className="container mt-4">
                <div className="row">
                    {products.map((item: CartItem) => (
                        <div
                            key={item.productId}
                            className="border rounded p-3 mb-3 shadow-sm"
                        >
                            <div className="row align-items-center">

                                <div className="col-md-4">
                                    <strong>{item.name}</strong>
                                </div>

                                <div className="col-md-2">
                                    {item.unitPrice} 
                                </div>

                                <div className="col-md-2">
                                    {item.quantity}
                                </div>

                                <div className="col-md-4 text-end">
                                    <strong>
                                        {(item.unitPrice * item.quantity).toFixed(2)}
                                    </strong>
                                </div>

                            </div>
                        </div>
                    ))}
                </div>
            </div>

            {products.length > 0 && (
                // sum up all the prices of products
                <p>Total sum: {totalPrice}</p>
            )}

            {products.length > 0 && (
                <button
                    onClick={() => { checkout(); }}>
                    Checkout
                </button>
            )}

            {products.length == 0 && (
                <div>
                    <p>Cart is empty...</p>
                    <p>Find products to add to your cart by</p>
                    <button onClick={goToProductsPage}>Clicking here</button>
                
                </div>
            )}
        </div>
    )
}

export default CartPage;