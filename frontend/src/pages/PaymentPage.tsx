import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";

function PaymentPage() {
    // state

    interface Payment {
        paymentId: string;
        orderId: string;
        status: string;
        paidAt: string;
        userId: string;
        totalPrice: number;
        items: PaymentItem[];
    };

    interface PaymentItem {
        paymentItemId: string;
        paymentId: string;
        productId: string;
        productName: string,
        unitPrice: number;
        quantity: number;
    };

    const [payment, setPayment] = useState<Payment | null>(null);
    const navigate = useNavigate();

    const { paymentId } = useParams();
    console.log(paymentId)

    // functions

    const pay = async (paymentProcessingId) => {
        try {
            const token = localStorage.getItem("token");

            const response = await fetch(
                "https://localhost:7144/api/payments",
                {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                        "Authorization": `Bearer ${token}`
                    },
                    body: JSON.stringify({
                        paymentId: paymentProcessingId
                    })
                });

            if (!response.ok) {
                const error = await response.json();
                alert(error.error);
                return;
            }
            console.log("payment completed succesfully")
        } catch (error) {
            console.error(error);
            alert("Could not connect to the server");
        }
    };
    

    useEffect(() => {

        let cancelled = false;
        const getPaymentDetails = async (paymentId) => {

            const token = localStorage.getItem("token");

            while (!cancelled)
            {
                const response = await fetch(
                    `https://localhost:7144/api/payments/${paymentId}`,
                    {
                        method: "GET",
                        headers: {
                            "Content-Type": "application/json",
                            "Authorization": `Bearer ${token}`
                        }
                    });


                if (response.ok) {
                    const data = await response.json();

                    if (!cancelled)
                        setPayment(data);
                    console.log(data)
                    break;
                }
                await new Promise(resolve => {
                    setTimeout(resolve, 1000)
                })
            };
        }
        getPaymentDetails(paymentId);

        return () => {
            cancelled = true;
        };

    }, [paymentId])

    if (!payment) {
        return <h2>Preparing payment...</h2>;
    }

    console.log(payment);

    // UI
    return (
        <div className="container mt-5">

            <h1>Payment</h1>

            <div className="card mb-4">
                <div className="card-body">

                    <h5 className="card-title">
                        Payment information
                    </h5>

                    <p>
                        <strong>Payment ID:</strong> {payment.paymentId}
                    </p>

                </div>
            </div>

            <h3>Items</h3>

            {/* Header */}
            <div className="row fw-bold border-bottom pb-2 mb-3">
                <div className="col-md-5">Product</div>
                <div className="col-md-2">Price</div>
                <div className="col-md-2">Quantity</div>
                <div className="col-md-3 text-end">Sum</div>
            </div>

            {/* Items */}
            {(payment).items.map((item: PaymentItem) => (
                <div
                    key={item.paymentItemId}
                    className="row align-items-center border-bottom py-3"
                >
                    <div className="col-md-5">
                        {item.productName}
                    </div>

                    <div className="col-md-2">
                        {item.unitPrice} kr
                    </div>

                    <div className="col-md-2">
                        {item.quantity}
                    </div>

                    <div className="col-md-3 text-end">
                        {(item.unitPrice * item.quantity).toFixed(2)} kr
                    </div>
                </div>
            ))}

            {/* Total */}
            <div className="card mt-4">
                <div className="card-body">

                    <div className="row">
                        <div className="col text-end">

                            <h4>
                                Total: {payment.totalPrice.toFixed(2)} kr
                            </h4>

                            <button
                                className="btn btn-success mt-3"
                                onClick={() =>
                                    {pay(paymentId)}
                                }>
                                Pay
                            </button>

                        </div>
                    </div>

                </div>
            </div>

        </div>
    );
}

export default PaymentPage;